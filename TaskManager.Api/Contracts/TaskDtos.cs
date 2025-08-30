namespace TaskManager.Api.Contracts;

public record TaskCreateRequest(string Title, string? Description, DateTime? DueDateUtc);

public record TaskUpdateRequest(string? Title, string? Description, bool? IsCompleted, DateTime? DueDateUtc);

public record TaskResponse(Guid Id, string Title, string? Description, bool IsCompleted, DateTime CreatedAt, DateTime? DueDateUtc);
