namespace TaskManager.Api.Endpoints.Contracts;


public record RegisterRequest(string Email, string UserName, string Password);