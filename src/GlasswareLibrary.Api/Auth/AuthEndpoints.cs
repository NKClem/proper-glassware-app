using System.ComponentModel.DataAnnotations;
using System.Text;
using GlasswareLibrary.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace GlasswareLibrary.Api.Auth;

public record LoginRequest([Required] string Email, [Required] string Password, bool RememberMe);

public record ForgotPasswordRequest([Required] string Email);

public record ResetPasswordRequest([Required] string Email, [Required] string Code, [Required] string NewPassword);

public static class AuthEndpoints
{
    public const string RateLimitPolicy = "auth";

    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder api)
    {
        var auth = api.MapGroup("/auth");

        auth.MapPost("/login", LoginAsync).AllowAnonymous().RequireRateLimiting(RateLimitPolicy);
        auth.MapPost("/logout", LogoutAsync).RequireAuthorization();
        auth.MapPost("/forgot-password", ForgotPasswordAsync).AllowAnonymous().RequireRateLimiting(RateLimitPolicy);
        auth.MapPost("/reset-password", ResetPasswordAsync).AllowAnonymous().RequireRateLimiting(RateLimitPolicy);

        return auth;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var result = await signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            return Results.Ok();
        }

        // Identity refuses a disabled account before it checks the password, so check the
        // password here before revealing that the account is disabled.
        if (result.IsNotAllowed && user.DisabledAt is not null && await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                detail: "This account is disabled.",
                extensions: new Dictionary<string, object?> { ["code"] = "account_disabled" });
        }

        return Results.Unauthorized();
    }

    private static async Task<IResult> LogoutAsync(SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.Ok();
    }

    private static async Task<IResult> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        UserManager<ApplicationUser> userManager,
        AccountEmails accountEmails)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || user.DisabledAt is not null)
        {
            return Results.Ok();
        }

        var hasLogins = (await userManager.GetLoginsAsync(user)).Count > 0;
        if (AccountState.IsUnclaimed(user, hasLogins))
        {
            await accountEmails.SendWelcomeAsync(user);
        }
        else if (user.EmailConfirmed)
        {
            await accountEmails.SendPasswordResetAsync(user);
        }

        return Results.Ok();
    }

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        var token = DecodeCode(request.Code);
        if (user is null || token is null)
        {
            return ValidationProblem([userManager.ErrorDescriber.InvalidToken()]);
        }

        var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
        if (!result.Succeeded)
        {
            return ValidationProblem(result.Errors);
        }

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        return Results.Ok();
    }

    private static string? DecodeCode(string code)
    {
        try
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static IResult ValidationProblem(IEnumerable<IdentityError> errors) =>
        Results.ValidationProblem(errors
            .GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
}
