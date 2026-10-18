using System.Text;
using GlasswareLibrary.Api.Common;
using GlasswareLibrary.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace GlasswareLibrary.Api.Auth;

public class AccountEmails(UserManager<ApplicationUser> userManager, IAccountEmailSender sender, IOptions<AppOptions> options)
{
    public async Task SendPasswordResetAsync(ApplicationUser user)
    {
        var link = await BuildResetLinkAsync(user);
        await sender.SendPasswordResetAsync(user, link);
    }

    public async Task SendWelcomeAsync(ApplicationUser user)
    {
        var link = await BuildResetLinkAsync(user) + "&welcome=1";
        string? createdByName = null;
        if (user.CreatedByUserId is { } creatorId)
        {
            createdByName = (await userManager.FindByIdAsync(creatorId.ToString()))?.DisplayName;
        }

        await sender.SendWelcomeAsync(user, link, createdByName);
    }

    private async Task<string> BuildResetLinkAsync(ApplicationUser user)
    {
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var baseUrl = options.Value.PublicBaseUrl.TrimEnd('/');
        return $"{baseUrl}/admin/auth/reset-password?email={Uri.EscapeDataString(user.Email!)}&code={code}";
    }
}
