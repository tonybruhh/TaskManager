namespace TaskManager.Api.Contracts;


public record AuthResponse(string Token, DateTime ExpiresAtUtc);