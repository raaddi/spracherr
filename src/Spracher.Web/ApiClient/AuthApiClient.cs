using Spracher.Contracts.Identity;

namespace Spracher.Web.ApiClient;

public sealed class AuthApiClient(JsonApiClient apiClient)
{
    public Task<ApiResult<AuthSessionResponse>> GetSessionAsync(
        CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<AuthSessionResponse>("api/v1/auth/session", cancellationToken);

    public Task<ApiResult<RegistrationResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<RegisterRequest, RegistrationResponse>(
            HttpMethod.Post,
            "api/v1/auth/register",
            request,
            cancellationToken);

    public Task<ApiResult<AuthSessionResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<LoginRequest, AuthSessionResponse>(
            HttpMethod.Post,
            "api/v1/auth/login",
            request,
            cancellationToken);

    public Task<ApiResult<OperationResponse>> LogoutAsync(
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<object, OperationResponse>(
            HttpMethod.Post,
            "api/v1/auth/logout",
            new { },
            cancellationToken);

    public Task<ApiResult<AuthenticatedUserResponse>> UpdateProfileAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<UpdateProfileRequest, AuthenticatedUserResponse>(
            HttpMethod.Put,
            "api/v1/profile/",
            request,
            cancellationToken);

    public Task<ApiResult<OperationResponse>> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<ConfirmEmailRequest, OperationResponse>(
            HttpMethod.Post,
            "api/v1/auth/confirm-email",
            request,
            cancellationToken);

    public Task<ApiResult<OperationResponse>> ResendConfirmationAsync(
        ResendConfirmationRequest request,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<ResendConfirmationRequest, OperationResponse>(
            HttpMethod.Post,
            "api/v1/auth/resend-confirmation",
            request,
            cancellationToken);

    public Task<ApiResult<OperationResponse>> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<ForgotPasswordRequest, OperationResponse>(
            HttpMethod.Post,
            "api/v1/auth/forgot-password",
            request,
            cancellationToken);

    public Task<ApiResult<OperationResponse>> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<ResetPasswordRequest, OperationResponse>(
            HttpMethod.Post,
            "api/v1/auth/reset-password",
            request,
            cancellationToken);
}
