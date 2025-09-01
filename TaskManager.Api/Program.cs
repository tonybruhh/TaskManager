using TaskManager.Api.Extensions;
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddRedisConnectionMultiplexer(builder.Configuration)
    .AddDataProtectionServices(builder.Configuration)
    .AddIdentityCoreServices()
    .AddEndpointsApiExplorer()
    .AddJwtAuth(builder.Configuration)
    .AddSwaggerGenForMinimalApi()
    .AddRequestValidation()
    .AddProblemDetails();


var app = builder.Build();

// Configure the HTTP request pipeline.
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

