namespace TaskManager.Api.Contracts;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int PageCount, int TotalCount);