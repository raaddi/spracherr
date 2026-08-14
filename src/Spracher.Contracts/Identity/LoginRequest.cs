namespace Spracher.Contracts.Identity;

public sealed record LoginRequest(
    string Email,
    string Password,
    bool RememberMe);
