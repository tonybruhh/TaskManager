using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Common;
using TaskManager.Api.Contracts;
using TaskManager.Api.Domain;
using TaskManager.Api.Extensions;
using TaskManager.Api.Infrastructure;


namespace TaskManager.Api.Endpoints;


public static class TaskEndpoints
{
    private const string CONCURRENCY_ERROR_MESSAGE = "The task was modified by another process.";

    #region Routing / Map
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/tasks").RequireAuthorization();

        group.MapPost("/", CreateTaskAsync).ValidateWith<TaskCreateRequest>();
        group.MapGet("/", GetTasksAsync);
        group.MapGet("/{id:guid}", GetTaskByIdAsync);
        group.MapPut("/{id:guid}", UpdateTaskAsync).ValidateWith<TaskUpdateRequest>();
        group.MapPost("/{id:guid}/complete", CompleteTaskAsync);
        group.MapDelete("/{id:guid}", DeleteTaskAsync);
        group.MapPost("/{id:guid}/restore", RestoreTaskAsync);
        group.MapGet("/_stats", GetStatsAsync);

        return app;
    }
    #endregion

    #region Route Handlers (CRUD)
    private static async Task<IResult> CreateTaskAsync(TaskCreateRequest req, AppDbContext db, ClaimsPrincipal user)
    {
        var userId = user.GetUserIdOrThrow();

        var task = new TaskItem(req.Title, userId, req.Description, req.DueDate);
        db.Tasks.Add(task);

        await db.SaveChangesAsync();

        return Results.Created($"/api/tasks/{task.Id}",
            new TaskResponse(task.Id, task.Title, task.Description, task.IsCompleted, task.CreatedAt, task.UpdatedAt, task.CompletedAt, task.DueDate));
    }

    private static async Task<IResult> GetTasksAsync(
        AppDbContext db,
        ClaimsPrincipal user,
        bool? isCompleted,
        DateTime? dueBefore,
        DateTime? dueAfter,
        DateTime? createdBefore,
        DateTime? createdAfter,
        string? search,
        string? sortings,
        int? page,
        int? pageSize)
    {
        var userId = user.GetUserIdOrThrow();

        var tasks = db.Tasks.AsNoTracking().Where(task => task.UserId == userId);

        tasks = SetFilters(tasks, isCompleted, dueBefore, dueAfter, createdBefore, createdAfter, search);
        tasks = SetSorting(tasks, sortings);

        var (p, ps) = Paging.Normalize(page, pageSize);
        var total = await tasks.CountAsync();
        var items = await tasks.Skip((p - 1) * ps).Take(ps)
            .Select(task => new TaskResponse(task.Id, task.Title, task.Description, task.IsCompleted, task.CreatedAt, task.UpdatedAt, task.CompletedAt, task.DueDate))
            .ToListAsync();

        return Results.Ok(new PagedResult<TaskResponse>(items, p, ps, items.Count, total));
    }

    private static async Task<IResult> GetTaskByIdAsync(Guid id, AppDbContext db, ClaimsPrincipal user)
    {
        var userId = user.GetUserIdOrThrow();

        var task = await db.Tasks.AsNoTracking()
            .Where(task => task.Id == id && task.UserId == userId)
            .Select(task => new TaskResponse(task.Id, task.Title, task.Description, task.IsCompleted, task.CreatedAt, task.UpdatedAt, task.CompletedAt, task.DueDate))
            .FirstOrDefaultAsync();

        return task is null ? Results.NotFound() : Results.Ok(task);
    }

    private static async Task<IResult> UpdateTaskAsync(Guid id, TaskUpdateRequest req, AppDbContext db, ClaimsPrincipal user)
    {
        var userId = user.GetUserIdOrThrow();

        var task = await db.Tasks.FirstOrDefaultAsync(task => task.Id == id && task.UserId == userId);

        if (task is null)
            return Results.NotFound();

        task.Title = req.Title ?? task.Title;
        task.Description = req.Description ?? task.Description;
        task.IsCompleted = req.IsCompleted ?? task.IsCompleted;
        task.DueDate = req.DueDate ?? task.DueDate;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Results.Conflict(new { error = CONCURRENCY_ERROR_MESSAGE });
        }

        return Results.Ok(new TaskResponse(task.Id, task.Title, task.Description, task.IsCompleted, task.CreatedAt, task.UpdatedAt, task.CompletedAt, req.DueDate));
    }

    private static async Task<IResult> CompleteTaskAsync(Guid Id, AppDbContext db, ClaimsPrincipal user)
    {
        var userId = user.GetUserIdOrThrow();

        var dto = await db.Tasks
            .Where(t => t.Id == Id && t.UserId == userId)
            .Select(t => new
            {
                t.Id,
                t.IsCompleted,
                Xmin = EF.Property<uint>(t, "xmin")
            })
            .SingleOrDefaultAsync();

        if (dto is null)
            return Results.NotFound();

        if (dto.IsCompleted)
            return Results.Ok();

        var stub = TaskItem.CreateStub(dto.Id);
        db.Attach(stub);

        stub.IsCompleted = true;

        db.Entry(stub).Property(x => x.IsCompleted).IsModified = true;
        db.Entry(stub).Property("xmin").OriginalValue = dto.Xmin;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Results.Conflict(new { error = CONCURRENCY_ERROR_MESSAGE });
        }

        return Results.Ok();    
    }

    private static async Task<IResult> DeleteTaskAsync(Guid id, AppDbContext db, ClaimsPrincipal user)
    {
        var userId = user.GetUserIdOrThrow();

        var dto = await db.Tasks
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(t => new
            {
                t.Id,
                t.IsDeleted,
                Xmin = EF.Property<uint>(t, "xmin")
            })
            .SingleOrDefaultAsync();

        if (dto is null)
            return Results.NotFound();

        if (dto.IsDeleted)
            return Results.NoContent();

        var stub = TaskItem.CreateStub(id);

        db.Attach(stub);

        stub.IsDeleted = true;

        db.Entry(stub).Property(t => t.IsDeleted).IsModified = true;
        db.Entry(stub).Property("xmin").OriginalValue = dto.Xmin;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Results.Conflict(new { error = CONCURRENCY_ERROR_MESSAGE });
        }

        return Results.NoContent();
    }

    private static async Task<IResult> RestoreTaskAsync(Guid id, AppDbContext db, ClaimsPrincipal user)
    {
        var userId = user.GetUserIdOrThrow();

        var dto = await db.Tasks
            .IgnoreQueryFilters()
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(t => new
            {
                t.Id,
                t.IsDeleted,
                Xmin = EF.Property<uint>(t, "xmin")
            })
            .SingleOrDefaultAsync();

        if (dto is null)
            return Results.NotFound();

        if (!dto.IsDeleted)
            return Results.Ok(new { restored = false, warning = "The task is not deleted" });

        var stub = TaskItem.CreateStub(id);

        db.Attach(stub);

        stub.IsDeleted = false;

        db.Entry(stub).Property(t => t.IsDeleted).IsModified = true;
        db.Entry(stub).Property("xmin").OriginalValue = dto.Xmin;

        try
        {
            await db.SaveChangesAsync();
        }
        catch
        {
            Results.Conflict(new { error = CONCURRENCY_ERROR_MESSAGE });
        }
        
        return Results.Ok(new { restored = true });
    }

    private static async Task<IResult> GetStatsAsync(AppDbContext db, ClaimsPrincipal user)
    {
        var userId = user.GetUserIdOrThrow();

        var now = DateTime.UtcNow;
        var total = await db.Tasks.CountAsync(t => t.UserId == userId);
        var done  = await db.Tasks.CountAsync(t => t.UserId == userId && t.IsCompleted);
        var open  = total - done;
        var overdue = await db.Tasks.CountAsync(t =>
            t.UserId == userId && !t.IsCompleted && t.DueDate != null && t.DueDate < now);

        return Results.Ok(new { total, open, done, overdue });
    }
    #endregion

    #region Filters & Sorting Helpers
    private static IQueryable<TaskItem> SetFilters(
        IQueryable<TaskItem> tasks,
        bool? isCompleted,
        DateTime? dueBefore,
        DateTime? dueAfter,
        DateTime? createdBefore,
        DateTime? createdAfter,
        string? search)
    {
        if (isCompleted is not null) tasks = tasks.Where(t => t.IsCompleted == isCompleted.Value);
        if (dueBefore is not null) tasks = tasks.Where(t => t.DueDate != null && t.DueDate < dueBefore);
        if (dueAfter is not null) tasks = tasks.Where(t => t.DueDate != null && t.DueDate > dueAfter);
        if (createdBefore is not null) tasks = tasks.Where(t => t.CreatedAt < createdBefore);
        if (createdAfter is not null) tasks = tasks.Where(t => t.CreatedAt > createdAfter);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            tasks = tasks.Where(t =>
                EF.Functions.ILike(t.Title, $"%{s}%") ||
                EF.Functions.ILike(t.Description!, $"%{s}%"));
        }

        return tasks;
    }

    private static IQueryable<TaskItem> SetSorting(IQueryable<TaskItem> tasks, string? sortings)
    {
        if (string.IsNullOrWhiteSpace(sortings))
            return tasks;

        IOrderedQueryable<TaskItem>? ordered = null;
        foreach (var sorting in sortings.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (ordered is null)
            {
                ordered = sorting switch
                {
                    "created" => tasks.OrderBy(t => t.CreatedAt).ThenBy(t => t.Id),
                    "-created" => tasks.OrderByDescending(t => t.CreatedAt).ThenByDescending(t => t.Id),
                    "udpated" => tasks.OrderBy(t => t.UpdatedAt),
                    "-updated" => tasks.OrderByDescending(t => t.UpdatedAt),
                    "completed" => tasks.OrderBy(t => t.CompletedAt),
                    "-completed" => tasks.OrderByDescending(t => t.CompletedAt),
                    "due" => tasks.OrderBy(t => t.DueDate),
                    "-due" => tasks.OrderByDescending(t => t.DueDate),
                    "title" => tasks.OrderBy(t => t.Title),
                    "-title" => tasks.OrderByDescending(t => t.Title),
                    _ => ordered
                };
            }
            else
            {
                ordered = sorting switch
                {
                    "created" => ordered.ThenBy(t => t.CreatedAt).ThenBy(t => t.Id),
                    "-created" => ordered.ThenByDescending(t => t.CreatedAt).ThenByDescending(t => t.Id),
                    "udpated" => ordered.ThenBy(t => t.UpdatedAt),
                    "-updated" => ordered.ThenByDescending(t => t.UpdatedAt),
                    "completed" => ordered.ThenBy(t => t.CompletedAt),
                    "-completed" => ordered.ThenByDescending(t => t.CompletedAt),
                    "due" => ordered.ThenBy(t => t.DueDate),
                    "-due" => ordered.ThenByDescending(t => t.DueDate),
                    "title" => ordered.ThenBy(t => t.Title),
                    "-title" => ordered.ThenByDescending(t => t.Title),
                    _ => ordered
                };
            }
        }

        return ordered ?? tasks;
    }
    #endregion
}