using System.Net.Http.Headers;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManager.Api.Infrastructure;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace TaskManager.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("task_manager_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .WithCommand("redis-server", "--requirepass", "redis")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Ready to accept connections tcp"))
        .Build();

    public async Task InitializeAsync()
    {
        await _pg.StartAsync();
        await _redis.StartAsync();

        var pg = $"Host={_pg.Hostname};Port={_pg.GetMappedPublicPort(5432)};Database=task_manager_test;Username=postgres;Password=postgres";
        var redis = $"{_redis.Hostname}:{_redis.GetMappedPublicPort(6379)},password=redis,abortConnect=false,connectTimeout=10000,syncTimeout=10000";

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://127.0.0.1:0");
        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres", pg);
        Environment.SetEnvironmentVariable("ConnectionStrings__Redis", redis);
        Environment.SetEnvironmentVariable("JWT__Key", "TaskManagerTaskManagerTaskManagerTaskManagerTaskManager");
        Environment.SetEnvironmentVariable("JWT__Issuer", "TaskManager");
        Environment.SetEnvironmentVariable("JWT__Audience", "TaskManager");
        Environment.SetEnvironmentVariable("DataProtection__AppName", "TaskManager.Tests");
        Environment.SetEnvironmentVariable("DataProtection__KeyLifetimeDays", "7");
        Environment.SetEnvironmentVariable("DataProtection__Cert__PfxPath", "../../cert/dpkeys.pfx");
        Environment.SetEnvironmentVariable("DataProtection__Cert__PfxPassword", "PfxPassword");
    }

    public new async Task DisposeAsync()
    {
        await _pg.DisposeAsync().AsTask();
        await _redis.DisposeAsync().AsTask();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting(WebHostDefaults.ServerUrlsKey, "http://127.0.0.1:0");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        return host;
    }
}
public static class HttpClientJwt
{
    public static void UseBearer(this HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
}
