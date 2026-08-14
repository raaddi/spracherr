using Spracher.Contracts.Exercises;

namespace Spracher.Web.ApiClient;

public sealed class ExercisesApiClient(JsonApiClient apiClient)
{
    public Task<ApiResult<ExerciseCatalogResponse>> GetCatalogAsync(
        CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<ExerciseCatalogResponse>(
            "api/v1/exercises/",
            cancellationToken);

    public Task<ApiResult<ExercisePlayResponse>> StartAttemptAsync(
        Guid definitionId,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<ExercisePlayResponse>(
            HttpMethod.Post,
            $"api/v1/exercises/{definitionId:D}/attempts",
            cancellationToken);

    public Task<ApiResult<ExerciseResultResponse>> SubmitAttemptAsync(
        Guid attemptId,
        SubmitExerciseAttemptRequest request,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<SubmitExerciseAttemptRequest, ExerciseResultResponse>(
            HttpMethod.Post,
            $"api/v1/exercise-attempts/{attemptId:D}/submit",
            request,
            cancellationToken);
}
