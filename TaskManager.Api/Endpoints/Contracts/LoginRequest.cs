namespace TaskManager.Api.Endpoints.Contracts;


public record LoginRequest(string EmailOrUserName, string Password);