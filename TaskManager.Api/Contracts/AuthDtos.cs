namespace TaskManager.Api.Contracts;


public record AuthResponse(string Token, DateTime ExpiresAtUtc);

public record LoginRequest(string EmailOrUserName, string Password);

public record RegisterRequest(string Email, string UserName, string Password);