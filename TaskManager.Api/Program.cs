using TaskManager.Api.Extensions;
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddRedisConnectionMultiplexer(builder.Configuration)
    .AddDataProtectionServices(builder.Configuration, builder.Environment)
    .AddIdentityCoreServices()
    .AddEndpointsApiExplorer()
    .AddJwtAuth(builder.Configuration)
    .AddSwaggerGenForMinimalApi()
    .AddRequestValidation()
    .AddHealthChecksServices(builder.Configuration)
    .AddProblemDetails();


var app = builder.Build();
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapApi();

app.Run();

public partial class Program { }