namespace TaskManager.Api.Contracts;


public record RegisterRequest(string Email, string UserName, string Password);