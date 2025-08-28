namespace TaskManager.Api.Contracts;


public record LoginRequest(string EmailOrUserName, string Password);