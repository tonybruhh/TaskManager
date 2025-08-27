namespace TaskManager.Api.Endpoints.Contracts;


public record AuthResponse(string Token, DateTime ExpiresAtUtc);