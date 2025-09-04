using TaskManager.Api.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services
    .AddPersistence(builder.Configuration)
    .AddRedisConnectionMultiplexer(builder.Configuration)
    .AddDataProtectionServices(builder.Configuration, builder.Environment)
    .AddCorsServices(builder.Configuration)
    .AddIdentityCoreServices()
    .AddEndpointsApiExplorer()
    .AddJwtAuth(builder.Configuration)
    .AddSwaggerGenForMinimalApi()
    .AddRequestValidation()
    .AddHealthChecksServices(builder.Configuration)
    .AddProblemDetails()
    .AddRateLimiterServices();


var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseRateLimiter();

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
app.UseCors();

app.MapApi();

app.Run();

public partial class Program { }