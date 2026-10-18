using GlasswareLibrary.Api.Data.Entities;

namespace GlasswareLibrary.Api.Auth;

public interface IAccountEmailSender
{
    Task SendPasswordResetAsync(ApplicationUser user, string resetLink);
    Task SendWelcomeAsync(ApplicationUser user, string welcomeLink, string? createdByName);
}
