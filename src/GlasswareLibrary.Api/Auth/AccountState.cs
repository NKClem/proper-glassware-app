using GlasswareLibrary.Api.Data.Entities;

namespace GlasswareLibrary.Api.Auth;

public static class AccountState
{
    public static bool IsUnclaimed(ApplicationUser user, bool hasLogins) =>
        user.PasswordHash is null && !hasLogins && !user.EmailConfirmed;
}
