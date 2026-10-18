using GlasswareLibrary.Api.Auth;
using GlasswareLibrary.Api.Data.Entities;

namespace GlasswareLibrary.Api.Tests;

public enum EmailKind
{
    PasswordReset,
    Welcome
}

public record SentEmail(EmailKind Kind, string To, string Link, string? CreatedByName);

public class FakeEmailSender : IAccountEmailSender
{
    private readonly List<SentEmail> _sent = [];

    public Task SendPasswordResetAsync(ApplicationUser user, string resetLink)
    {
        Record(new SentEmail(EmailKind.PasswordReset, user.Email!, resetLink, null));
        return Task.CompletedTask;
    }

    public Task SendWelcomeAsync(ApplicationUser user, string welcomeLink, string? createdByName)
    {
        Record(new SentEmail(EmailKind.Welcome, user.Email!, welcomeLink, createdByName));
        return Task.CompletedTask;
    }

    public IReadOnlyList<SentEmail> SentTo(string email)
    {
        lock (_sent)
        {
            return _sent.Where(e => string.Equals(e.To, email, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }

    private void Record(SentEmail email)
    {
        lock (_sent)
        {
            _sent.Add(email);
        }
    }
}
