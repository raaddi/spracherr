namespace Spracher.Contracts.Identity;

public sealed record ResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword);
