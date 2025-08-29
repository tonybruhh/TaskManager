using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Contracts;
using TaskManager.Api.Domain;
using TaskManager.Api.Extensions;
using TaskManager.Api.Infrastructure;

namespace TaskManager.Api.Endpoints;


public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/tasks").RequireAuthorization();

        group.MapPost("/", CreateTaskAsync).ValidateWith<TaskCreateRequest>();
        group.MapGet("/", GetTasksAsync);
        group.MapGet("/{id:guid}", GetTaskByIdAsync);
        group.MapPut("/{id:guid}", UpdateTaskAsync).ValidateWith<TaskUpdateRequest>();
        group.MapDelete("/{id:guid}", DeleteTaskAsync);

        return app;
    }

    private static async Task<IResult> CreateTaskAsync(TaskCreateRequest req, AppDbContext db, ClaimsPrincipal user)
    {
        var userId = user.GetUserIdOrThrow();

        var task = new TaskItem(req.Title, userId, req.Description, req.DueDateUtc);
        db.Tasks.Add(task);

        await db.SaveChangesAsync();

        return Results.Created($"/api/tasks/{task.Id}",
            new TaskResponse(task.Id, task.Title, task.Description, task.IsCompleted, task.DueDateUtc));
    }

    private static async Task<IResult> GetTasksAsync(AppDbContext db, ClaimsPrincipal user)
    {
        var userId = user.GetUserIdOrThrow();

        var tasks = await db.Tasks.AsNoTracking()
            .Where(task => task.UserId == userId)
            .Select(task => new TaskResponse(task.Id, task.Title, task.Description, task.IsCompleted, task.DueDateUtc))
            .ToListAsync();

        return Results.Ok(tasks);
    }

    private static async Task<IResult> GetTaskByIdAsync(Guid id, AppDbContext db, ClaimsPrincipal user)
    {
        user.IsUserIdNullOrEmpty();

        var task = await db.Tasks.AsNoTracking()
            .Where(task => task.Id == id)
            .Select(task => new TaskResponse(task.Id, task.Title, task.Description, task.IsCompleted, task.DueDateUtc))
            .FirstOrDefaultAsync();

        return task is null ? Results.NotFound() : Results.Ok(task);
    }

    private static async Task<IResult> UpdateTaskAsync(Guid id, TaskUpdateRequest req, AppDbContext db, ClaimsPrincipal user)
    {
        user.IsUserIdNullOrEmpty();

        var task = await db.Tasks.FirstOrDefaultAsync(task => task.Id == id);

        if (task is null)
            return Results.NotFound();

        task.Title = req.Title ?? task.Title;
        task.Description = req.Description ?? task.Description;
        task.IsCompleted = req.IsCompleted ?? task.IsCompleted;
        task.DueDateUtc = req.DueDateUtc ?? task.DueDateUtc;

        await db.SaveChangesAsync();

        return Results.Ok(new TaskResponse(task.Id, task.Title, task.Description, task.IsCompleted, req.DueDateUtc));
    }

    private static async Task<IResult> DeleteTaskAsync(Guid id, AppDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Results.Unauthorized();

        await db.Tasks.Where(task => task.Id == id).ExecuteDeleteAsync();

        return Results.NoContent();
    }
}