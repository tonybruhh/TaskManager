using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using TaskManager.Api.Domain;
using TaskManager.Api.Infrastructure;
using TaskManager.Api.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;

namespace TaskManager.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration cfg)
    {
        var connectionString = cfg.GetConnectionString("Postgres");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "ConnectionStrings:Postgres is not configured. Provide env var 'ConnectionStrings__Postgres'.");
        }

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }

    public static IServiceCollection AddSwaggerGenForMinimalApi(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskManager API", Version = "v1" });
            var jwtScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Input: Bearer {your token}"
            };
            options.AddSecurityDefinition("Bearer", jwtScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddIdentityCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(TimeProvider.System);

        services
        .AddIdentityCore<AppUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 12;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration cfg)
    {
        services.Configure<JwtOptions>(cfg.GetSection(JwtOptions.SectionName));

        var jwt = cfg.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwt.Key))
        {
            throw new ArgumentNullException(nameof(jwt.Key), "JWT:Key is not configured (env var 'JWT__Key').");
        }

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = signingKey,
                ClockSkew = TimeSpan.Zero,

                NameClaimType = JwtRegisteredClaimNames.UniqueName,
                RoleClaimType = ClaimTypes.Role
            };
        });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddRedisConnectionMultiplexer(this IServiceCollection services, IConfiguration cfg)
    {
        var connectionString = cfg.GetConnectionString("Redis") ?? throw new InvalidOperationException("Missing ConnectionStrings:Redis");
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionString));

        return services;
    }

    public static IServiceCollection AddDataProtectionServices(this IServiceCollection services, IConfiguration cfg, IHostEnvironment env)
    {
        services.Configure<DataProtectionApiOptions>(cfg.GetSection(DataProtectionApiOptions.SectionName));
        var dp = cfg.GetSection(DataProtectionApiOptions.SectionName).Get<DataProtectionApiOptions>() ?? new();

        var dataProtectionServices = services.AddDataProtection()
            .SetApplicationName(dp.AppName ?? "TaskManager")
            .SetDefaultKeyLifetime(TimeSpan.FromDays(dp.KeyLifetimeDays > 0 ? dp.KeyLifetimeDays : 30));

        if (!env.IsDevelopment() && !env.IsEnvironment(Constants.TestingEnvironment))
        {
            var pfxPath = cfg["DataProtection:Cert:PfxPath"];
            var pfxPwd = cfg["DataProtection:Cert:PfxPassword"];

            if (string.IsNullOrWhiteSpace(pfxPath) || string.IsNullOrWhiteSpace(pfxPwd))
                throw new InvalidOperationException("Configure DataProtection:Cert:PfxPath & PfxPassword");

            var cert = new X509Certificate2(pfxPath, pfxPwd, X509KeyStorageFlags.MachineKeySet);
            dataProtectionServices.ProtectKeysWithCertificate(cert);
        }

        services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(sp =>
            new ConfigureOptions<KeyManagementOptions>(o =>
            {
                var mux = sp.GetRequiredService<IConnectionMultiplexer>();
                o.XmlRepository = new RedisXmlRepository(() => mux.GetDatabase(), "DataProtection-Keys");
            }));

        return services;
    }

    public static IServiceCollection AddHealthChecksServices(this IServiceCollection services, IConfiguration cfg)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(cfg.GetConnectionString("Postgres")!)
            .AddRedis(cfg.GetConnectionString("Redis")!);

        return services;
    }

    public static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddCors(o =>
        {
            o.AddPolicy(Constants.CorsAllowFrontend, p => p
                .WithOrigins(cfg["Cors:Frontend"] ?? "http://localhost:5173", "http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
        });

        services.AddCors(o =>
        {
            o.AddPolicy(Constants.CorsAllowAll, p => p
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

        return services;
    }

    public static IServiceCollection AddRateLimiterServices(this IServiceCollection services)
    {
        services.AddRateLimiter(o =>
        {
            o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ctx.User.Identity?.Name ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "anon",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }
                ));
            o.RejectionStatusCode = 429;
        });

        return services;
    }
}