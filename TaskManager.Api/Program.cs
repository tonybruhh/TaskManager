using TaskManager.Api.Extentions;
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddIdentityCoreServices()
    .AddEndpointsApiExplorer()
    .AddJwtAuth(builder.Configuration)
    .AddSwaggerGenForMinimalApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapApi();

app.MapGet("/ping", () => Results.Ok(new { ok = true, time = DateTime.UtcNow }))
    .WithName("Ping")
    .WithOpenApi()
    .RequireAuthorization();

app.Run();

