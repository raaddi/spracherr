using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Spracher.BuildingBlocks.Time;
using Spracher.Contracts.Identity;
using Spracher.IdentityModel;
using Spracher.Modules.IdentityAccess.Email;

namespace Spracher.Modules.IdentityAccess.Application;

internal sealed class IdentityAccountService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IIdentityEmailSender emailSender,
    IdentityLinkFactory linkFactory,
    IClock clock)
{
    public async Task<AccountResult<RegistrationResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationErrors = AccountRequestValidator.ValidateRegistration(
            request.Email,
            request.Password,
            request.DisplayName,
            request.TimeZoneId);
        if (validationErrors.Count > 0)
        {
            return AccountResult<RegistrationResponse>.Validation(validationErrors);
        }

        var email = request.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return AccountResult<RegistrationResponse>.Conflict(
                "email",
                "An account with this email address already exists.");
        }

        var user = ApplicationUser.Create(
            email,
            request.DisplayName,
            request.TimeZoneId,
            clock.UtcNow);
        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            if (createResult.Errors.Any(error =>
                    string.Equals(error.Code, "DuplicateEmail", StringComparison.Ordinal)
                    || string.Equals(error.Code, "DuplicateUserName", StringComparison.Ordinal)))
            {
                return AccountResult<RegistrationResponse>.Conflict(
                    "email",
                    "An account with this email address already exists.");
            }

            return AccountResult<RegistrationResponse>.Validation(ToErrors(createResult));
        }

        var roleResult = await userManager.AddToRoleAsync(user, SystemRoles.SelfLearner);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            throw new InvalidOperationException(
                "The default SelfLearner role is unavailable. Apply the latest migrations.");
        }

        await SendEmailConfirmationAsync(user, cancellationToken);

        return AccountResult<RegistrationResponse>.Success(
            new RegistrationResponse(RequiresEmailConfirmation: true));
    }

    public async Task<AccountResult<AuthSessionResponse>> LoginAsync(LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!AccountRequestValidator.IsValidEmail(request.Email)
            || string.IsNullOrWhiteSpace(request.Password))
        {
            return AccountResult<AuthSessionResponse>.Unauthorized();
        }

        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || user.Status != AccountStatus.Active)
        {
            return AccountResult<AuthSessionResponse>.Unauthorized();
        }

        var signInResult = await signInManager.PasswordSignInAsync(
            user,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            return AccountResult<AuthSessionResponse>.LockedOut();
        }

        if (signInResult.IsNotAllowed)
        {
            return AccountResult<AuthSessionResponse>.NotAllowed();
        }

        if (!signInResult.Succeeded)
        {
            return AccountResult<AuthSessionResponse>.Unauthorized();
        }

        return AccountResult<AuthSessionResponse>.Success(
            new AuthSessionResponse(
                IsAuthenticated: true,
                User: await CreateUserResponseAsync(user)));
    }

    public async Task<AuthSessionResponse> GetSessionAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.Identity?.IsAuthenticated != true)
        {
            return new AuthSessionResponse(IsAuthenticated: false, User: null);
        }

        var user = await userManager.GetUserAsync(principal);
        return user is null
            ? new AuthSessionResponse(IsAuthenticated: false, User: null)
            : new AuthSessionResponse(
                IsAuthenticated: true,
                User: await CreateUserResponseAsync(user));
    }

    public Task LogoutAsync() => signInManager.SignOutAsync();

    public async Task<AccountResult<AuthSessionResponse>> UpdateProfileAsync(
        ClaimsPrincipal principal,
        UpdateProfileRequest request)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(request);

        var validationErrors = AccountRequestValidator.ValidateProfile(
            request.DisplayName,
            request.TimeZoneId);
        if (validationErrors.Count > 0)
        {
            return AccountResult<AuthSessionResponse>.Validation(validationErrors);
        }

        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return AccountResult<AuthSessionResponse>.Unauthorized();
        }

        user.UpdateProfile(request.DisplayName, request.TimeZoneId, clock.UtcNow);
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return AccountResult<AuthSessionResponse>.Validation(ToErrors(updateResult));
        }

        await signInManager.RefreshSignInAsync(user);

        return AccountResult<AuthSessionResponse>.Success(
            new AuthSessionResponse(
                IsAuthenticated: true,
                User: await CreateUserResponseAsync(user)));
    }

    public async Task<AccountResult<OperationResponse>> ConfirmEmailAsync(
        ConfirmEmailRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null || !TryDecodeToken(request.Code, out var token))
        {
            return InvalidConfirmationResult();
        }

        if (user.EmailConfirmed)
        {
            return AccountResult<OperationResponse>.Success(
                new OperationResponse("Email address is already confirmed."));
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded
            ? AccountResult<OperationResponse>.Success(
                new OperationResponse("Email address confirmed."))
            : InvalidConfirmationResult();
    }

    public async Task<AccountResult<OperationResponse>> ResendConfirmationAsync(
        ResendConfirmationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (AccountRequestValidator.IsValidEmail(request.Email))
        {
            var user = await userManager.FindByEmailAsync(request.Email.Trim());
            if (user is not null && !user.EmailConfirmed)
            {
                await SendEmailConfirmationAsync(user, cancellationToken);
            }
        }

        return AccountResult<OperationResponse>.Accepted(
            new OperationResponse(
                "If the account exists, a confirmation message has been sent."));
    }

    public async Task<AccountResult<OperationResponse>> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (AccountRequestValidator.IsValidEmail(request.Email))
        {
            var user = await userManager.FindByEmailAsync(request.Email.Trim());
            if (user is not null && user.EmailConfirmed)
            {
                await SendPasswordResetAsync(user, cancellationToken);
            }
        }

        return AccountResult<OperationResponse>.Accepted(
            new OperationResponse(
                "If the account exists, password reset instructions have been sent."));
    }

    public async Task<AccountResult<OperationResponse>> ResetPasswordAsync(
        ResetPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!AccountRequestValidator.IsValidEmail(request.Email)
            || string.IsNullOrWhiteSpace(request.Code)
            || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return InvalidPasswordResetResult();
        }

        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !TryDecodeToken(request.Code, out var token))
        {
            return InvalidPasswordResetResult();
        }

        var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
        return result.Succeeded
            ? AccountResult<OperationResponse>.Success(
                new OperationResponse("Password has been reset."))
            : AccountResult<OperationResponse>.Validation(ToErrors(result));
    }

    private static AccountResult<OperationResponse> InvalidConfirmationResult() =>
        AccountResult<OperationResponse>.Validation(
            new Dictionary<string, string[]>
            {
                ["code"] = ["The email confirmation link is invalid or has expired."],
            });

    private static AccountResult<OperationResponse> InvalidPasswordResetResult() =>
        AccountResult<OperationResponse>.Validation(
            new Dictionary<string, string[]>
            {
                ["code"] = ["The password reset request is invalid or has expired."],
            });

    private static Dictionary<string, string[]> ToErrors(IdentityResult result) =>
        result.Errors
            .GroupBy(error => error.Code, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray(),
                StringComparer.Ordinal);

    private static bool TryDecodeToken(string? encodedToken, out string token)
    {
        token = string.Empty;
        if (string.IsNullOrWhiteSpace(encodedToken))
        {
            return false;
        }

        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedToken));
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private async Task<AuthenticatedUserResponse> CreateUserResponseAsync(
        ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new AuthenticatedUserResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.TimeZoneId,
            roles.Order(StringComparer.Ordinal).ToArray());
    }

    private async Task SendEmailConfirmationAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var rawToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
        var actionUrl = linkFactory.CreateEmailConfirmationUrl(user.Id, encodedToken);
        var encodedUrl = HtmlEncoder.Default.Encode(actionUrl.AbsoluteUri);

        await emailSender.SendAsync(
            new IdentityEmailMessage(
                Recipient: user.Email ?? throw new InvalidOperationException("User email is missing."),
                Subject: "Potwierdź adres e-mail w Spracher",
                PlainTextBody: $"Potwierdź adres e-mail: {actionUrl.AbsoluteUri}",
                HtmlBody: $"<p>Potwierdź adres e-mail, otwierając <a href=\"{encodedUrl}\">ten link</a>.</p>",
                ActionUrl: actionUrl.AbsoluteUri,
                CreatedAt: clock.UtcNow),
            cancellationToken);
    }

    private async Task SendPasswordResetAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var rawToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
        var email = user.Email ?? throw new InvalidOperationException("User email is missing.");
        var actionUrl = linkFactory.CreatePasswordResetUrl(email, encodedToken);
        var encodedUrl = HtmlEncoder.Default.Encode(actionUrl.AbsoluteUri);

        await emailSender.SendAsync(
            new IdentityEmailMessage(
                Recipient: email,
                Subject: "Reset hasła w Spracher",
                PlainTextBody: $"Ustaw nowe hasło: {actionUrl.AbsoluteUri}",
                HtmlBody: $"<p>Ustaw nowe hasło, otwierając <a href=\"{encodedUrl}\">ten link</a>.</p>",
                ActionUrl: actionUrl.AbsoluteUri,
                CreatedAt: clock.UtcNow),
            cancellationToken);
    }
}
