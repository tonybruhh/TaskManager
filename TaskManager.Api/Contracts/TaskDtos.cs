namespace TaskManager.Api.Contracts;

public record TaskCreateRequest(string Title, string? Description, DateTimeOffset? DueDate);

public record TaskUpdateRequest(string? Title, string? Description, bool? IsCompleted, DateTimeOffset? DueDate);

public record TaskResponse(Guid Id, string Title, string? Description, bool IsCompleted, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, DateTimeOffset? DueDate);
