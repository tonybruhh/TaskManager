using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Api.Configuration;
using TaskManager.Api.Domain;
using TaskManager.Api.Contracts;


namespace TaskManager.Api.Endpoints;


public static class AuthEndPoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (RegisterRequest req, UserManager<AppUser> users) =>
        {
            var user = new AppUser { UserName = req.UserName, Email = req.Email, EmailConfirmed = true };
            var result = await users.CreateAsync(user, req.Password);
            return result.Succeeded
                ? Results.Ok()
                : Results.BadRequest(new { error = result.Errors.Select(e => e.Description) });
        });

        group.MapPost("/login", async (LoginRequest req, UserManager<AppUser> users, IOptions<JwtOptions> jwtOpt) =>
        {
            var user = await users.FindByEmailAsync(req.EmailOrUserName)
                        ?? await users.FindByNameAsync(req.EmailOrUserName);

            if (user is null || !await users.CheckPasswordAsync(user, req.Password))
                return Results.Unauthorized();

            var jwt = jwtOpt.Value;
            var token = CreateJwt(user, jwt);

            return Results.Ok(new AuthResponse(token.Token, token.ExpiresAtUtc));
        });

        group.MapGet("/me", [Authorize](ClaimsPrincipal user) =>
        {
            var id = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var name = user.Identity?.Name ?? user.FindFirstValue(JwtRegisteredClaimNames.UniqueName);
            var email = user.FindFirstValue(JwtRegisteredClaimNames.Email);

            return Results.Ok(new {Id = id, UserName = name, Email = email });
        });

        return app;
    }

    private static (string Token, DateTime ExpiresAtUtc) CreateJwt(AppUser user, JwtOptions jwt)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(jwt.ExpiresMinutes);

        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.Id),
            new (JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new (JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),

            new (ClaimTypes.NameIdentifier, user.Id),
            new (ClaimTypes.Name, user.UserName ?? string.Empty),
            new (ClaimTypes.Email, user.Email ?? string.Empty)
        };

        var token = new JwtSecurityToken(issuer: jwt.Issuer,
                                         audience: jwt.Audience,
                                         claims: claims,
                                         expires: expires,
                                         signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}