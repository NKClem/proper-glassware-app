using GlasswareLibrary.Api.Data.Entities;

namespace GlasswareLibrary.Api.Auth;

public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IAccountEmailSender
{
    public Task SendPasswordResetAsync(ApplicationUser user, string resetLink)
    {
        logger.LogInformation("Password reset email to {Email}: {Link}", user.Email, resetLink);
        return Task.CompletedTask;
    }

    public Task SendWelcomeAsync(ApplicationUser user, string welcomeLink, string? createdByName)
    {
        logger.LogInformation("Welcome email to {Email}, created by {CreatedBy}: {Link}", user.Email, createdByName, welcomeLink);
        return Task.CompletedTask;
    }
}
