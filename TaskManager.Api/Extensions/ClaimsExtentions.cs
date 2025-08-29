using System.Security.Claims;
using Microsoft.VisualBasic;

namespace TaskManager.Api.Extensions;


public static class ClaimsExtentions
{
    public static Guid GetUserIdOrThrow(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : throw new UnauthorizedAccessException("Missing NameIdentifier claim.");

    public static void IsUserIdNullOrEmpty(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? throw new UnauthorizedAccessException("Missing NameIdentifier claim."); 
    }
}