using System.Security.Claims;

namespace Backend.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.Identity.Name;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            throw new Exception("UserId claim not found or invalid.");
        }
        return userId;
    }
}
