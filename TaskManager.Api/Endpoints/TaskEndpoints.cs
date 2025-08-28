using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Configuration.UserSecrets;
using Npgsql.Internal;
using TaskManager.Api.Contracts;
using TaskManager.Api.Domain;
using TaskManager.Api.Infrastructure;

namespace TaskManager.Api.Endpoints;


public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MaptaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/tasks").RequireAuthorization();

        group.MapPost("/", async (TaskCreateRequest req, AppDbContext db, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Results.Unauthorized();

            var task = new TaskItem(req.Title, userId, req.Description, req.DueDateUtc);

            db.Tasks.Add(task);

            await db.SaveChangesAsync();

            return Results.Created($"/api/tasks/{task.Id}", 
                new TaskResponse(task.Id, task.Title, task.Description, task.IsCompleted, task.DueDateUtc));
        });

        group.MapGet("/", async (AppDbContext db, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Results.Unauthorized();

            var tasks = await db.Tasks.AsNoTracking()
                .Where(task => task.UserId == userId)
                .Select(task => new TaskResponse(task.Id, task.Title, task.Description, task.IsCompleted, task.DueDateUtc))
                .ToListAsync();


            return Results.Ok(tasks);
        });

        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Results.Unauthorized();

            var task = await db.Tasks.AsNoTracking()
                .Where(task =>task.Id == id)
                .Select(task => new TaskResponse(task.Id, task.Title, task.Description, task.IsCompleted, task.DueDateUtc))
                .FirstOrDefaultAsync();

            return task is null ? Results.NotFound() : Results.Ok(task);
        });

        group.MapPut("/{id:guid}", async (Guid id, TaskUpdateRequest req, AppDbContext db, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Results.Unauthorized();

            var task = await db.Tasks.FirstOrDefaultAsync(task => task.Id == id);

            if (task is null)
                return Results.NotFound();

            task.Title = req.Title ?? task.Title;
            task.Description = req.Description ?? task.Description;
            task.IsCompleted = req.IsCompleted ?? task.IsCompleted;
            task.DueDateUtc = req.DueDateUtc ?? task.DueDateUtc;

            await db.SaveChangesAsync();

            return Results.Ok(new TaskResponse(task.Id, task.Title, task.Description, task.IsCompleted, req.DueDateUtc));
        });

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Results.Unauthorized();

            await db.Tasks.Where(task => task.Id == id).ExecuteDeleteAsync();

            return Results.NoContent();
        });

        return app;
    }
}