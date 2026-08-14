using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Spracher.Api.IntegrationTests.Infrastructure;
using Spracher.Contracts.Identity;
using Spracher.Contracts.Languages;
using Spracher.Contracts.Vocabulary;

namespace Spracher.Api.IntegrationTests;

public sealed class IdentityAndLanguagesFlowTests(PostgresWebApplicationFactory factory)
    : IClassFixture<PostgresWebApplicationFactory>
{
    [IntegrationFact]
    public async Task ConfirmedUserShouldAuthenticateAndConfigureLanguages()
    {
        using var client = CreateSecureClient();
        var password = "Secure1!Alpha";
        await RegisterConfirmedAndLoginAsync(client, "Ada Learner", password);

        var session = await client.GetFromJsonAsync<AuthSessionResponse>(
            "/api/v1/auth/session");
        Assert.NotNull(session);
        Assert.True(session.IsAuthenticated);
        Assert.NotNull(session.User);
        Assert.Contains("SelfLearner", session.User.Roles);

        var profileUpdate = await SendWithAntiforgeryAsync(
            client,
            HttpMethod.Put,
            "/api/v1/profile/",
            new UpdateProfileRequest("Ada Updated", "UTC"));
        Assert.Equal(HttpStatusCode.OK, profileUpdate.StatusCode);
        var updatedUser = await profileUpdate.Content.ReadFromJsonAsync<AuthenticatedUserResponse>();
        Assert.Equal("Ada Updated", updatedUser?.DisplayName);

        var catalog = await client.GetFromJsonAsync<IReadOnlyList<LanguageResponse>>(
            "/api/v1/languages");
        Assert.NotNull(catalog);
        Assert.Equal(5, catalog.Count);
        var polish = Assert.Single(catalog, language => language.Code == "pl");
        var english = Assert.Single(catalog, language => language.Code == "en");

        var languageUpdate = await SendWithAntiforgeryAsync(
            client,
            HttpMethod.Put,
            "/api/v1/languages/me",
            new UpdateUserLanguagesRequest(
            [
                new UserLanguageSelectionRequest(polish.Id, IsNative: true, IsLearning: false),
                new UserLanguageSelectionRequest(english.Id, IsNative: false, IsLearning: true),
            ]));
        Assert.Equal(HttpStatusCode.OK, languageUpdate.StatusCode);
        var profiles = await languageUpdate.Content
            .ReadFromJsonAsync<IReadOnlyList<UserLanguageProfileResponse>>();
        Assert.NotNull(profiles);
        Assert.Equal("A0", Assert.Single(profiles, language => language.Code == "en").CurrentCefrLevel);
        Assert.True(Assert.Single(profiles, language => language.Code == "pl").IsNative);

        var search = await client.GetFromJsonAsync<VocabularySearchResponse>(
            $"/api/v1/vocabulary/search?languageId={english.Id}&query=bank");
        Assert.NotNull(search);
        var bank = Assert.Single(search.Items);
        Assert.Equal(2, bank.SenseCount);

        var bankDetails = await client.GetFromJsonAsync<VocabularyDetailsResponse>(
            $"/api/v1/vocabulary/lexemes/{bank.LexemeId}");
        Assert.NotNull(bankDetails);
        Assert.Equal(2, bankDetails.Senses.Count);
        Assert.Contains(
            bankDetails.Senses,
            sense => sense.Equivalents.Any(equivalent => equivalent.Lemma == "brzeg"));

        var financialSense = Assert.Single(
            bankDetails.Senses,
            sense => sense.Definition.Contains("organization", StringComparison.Ordinal));
        var addItem = await SendWithAntiforgeryAsync(
            client,
            HttpMethod.Post,
            "/api/v1/vocabulary/items",
            new AddVocabularyItemRequest(financialSense.SenseId));
        Assert.Equal(HttpStatusCode.OK, addItem.StatusCode);
        var addedItem = await addItem.Content.ReadFromJsonAsync<UserVocabularyItemResponse>();
        Assert.NotNull(addedItem);
        Assert.Equal("New", addedItem.Status);

        var updateStatus = await SendWithAntiforgeryAsync(
            client,
            HttpMethod.Put,
            $"/api/v1/vocabulary/items/{addedItem.Id}/status",
            new UpdateVocabularyStatusRequest("Learned"));
        Assert.Equal(HttpStatusCode.OK, updateStatus.StatusCode);
        var learnedItem = await updateStatus.Content
            .ReadFromJsonAsync<UserVocabularyItemResponse>();
        Assert.Equal("Learned", learnedItem?.Status);

        var privateLemma = $"moonword-{Guid.NewGuid():N}";
        var createPrivate = await SendWithAntiforgeryAsync(
            client,
            HttpMethod.Post,
            "/api/v1/vocabulary/private",
            new CreatePrivateVocabularyRequest(
                english.Id,
                privateLemma,
                "Noun",
                "A1",
                "Created during an integration test.",
                polish.Id,
                "prywatne słowo testowe"));
        Assert.Equal(HttpStatusCode.Created, createPrivate.StatusCode);

        var privateSearch = await client.GetFromJsonAsync<VocabularySearchResponse>(
            $"/api/v1/vocabulary/search?languageId={english.Id}&query={privateLemma}");
        Assert.NotNull(privateSearch);
        Assert.True(Assert.Single(privateSearch.Items).IsPrivate);

        using var anonymousClient = CreateSecureClient();
        var anonymousSearch = await anonymousClient
            .GetFromJsonAsync<VocabularySearchResponse>(
                $"/api/v1/vocabulary/search?languageId={english.Id}&query={privateLemma}");
        Assert.NotNull(anonymousSearch);
        Assert.Empty(anonymousSearch.Items);

        var userVocabulary = await client.GetFromJsonAsync<UserVocabularyResponse>(
            "/api/v1/vocabulary/me");
        Assert.NotNull(userVocabulary);
        Assert.Contains(userVocabulary.Items, item => item.Lemma == privateLemma);
        Assert.Contains(
            userVocabulary.Items,
            item => item.Lemma == "bank" && item.Status == "Learned");

        var createList = await SendWithAntiforgeryAsync(
            client,
            HttpMethod.Post,
            "/api/v1/vocabulary/lists",
            new CreateVocabularyListRequest("Travel", "Words for the next trip."));
        Assert.Equal(HttpStatusCode.Created, createList.StatusCode);
        var vocabularyList = await createList.Content
            .ReadFromJsonAsync<VocabularyListDetailsResponse>();
        Assert.NotNull(vocabularyList);

        var addToList = await SendWithAntiforgeryAsync(
            client,
            HttpMethod.Post,
            $"/api/v1/vocabulary/lists/{vocabularyList.Id}/items",
            new AddVocabularyListItemRequest(addedItem.Id, "Review before departure."));
        Assert.Equal(HttpStatusCode.OK, addToList.StatusCode);
        var updatedList = await addToList.Content
            .ReadFromJsonAsync<VocabularyListDetailsResponse>();
        Assert.Equal("bank", Assert.Single(updatedList?.Items ?? []).Lemma);

        var createCategory = await SendWithAntiforgeryAsync(
            client,
            HttpMethod.Post,
            "/api/v1/vocabulary/categories",
            new CreateVocabularyCategoryRequest("Difficult", "#B84D40"));
        Assert.Equal(HttpStatusCode.Created, createCategory.StatusCode);
        var category = await createCategory.Content
            .ReadFromJsonAsync<VocabularyCategoryResponse>();
        Assert.NotNull(category);

        var assignCategory = await SendWithAntiforgeryAsync(
            client,
            HttpMethod.Put,
            $"/api/v1/vocabulary/items/{addedItem.Id}/categories",
            new AssignVocabularyCategoriesRequest([category.Id]));
        Assert.Equal(HttpStatusCode.OK, assignCategory.StatusCode);

        var categories = await client.GetFromJsonAsync<VocabularyCategoriesResponse>(
            "/api/v1/vocabulary/me/categories");
        Assert.Contains(
            Assert.IsType<VocabularyCategoriesResponse>(categories).Items,
            item => item.Id == category.Id
                    && item.AssignedUserVocabularyItemIds.Contains(addedItem.Id));

        using var missingTokenRequest = JsonContent.Create(new UpdateProfileRequest("No CSRF", "UTC"));
        using var rejected = await client.PutAsync("/api/v1/profile/", missingTokenRequest);
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
    }

    [IntegrationFact]
    public async Task ProtectedLanguageProfileShouldRejectAnonymousUser()
    {
        using var client = CreateSecureClient();

        var response = await client.GetAsync("/api/v1/languages/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [IntegrationFact]
    public async Task PrivateVocabularyShouldBeIsolatedBetweenUsers()
    {
        using var ownerClient = CreateSecureClient();
        using var otherClient = CreateSecureClient();
        const string password = "Secure1!Alpha";
        await RegisterConfirmedAndLoginAsync(ownerClient, "Vocabulary Owner", password);
        await RegisterConfirmedAndLoginAsync(otherClient, "Other Learner", password);

        var catalog = await ownerClient.GetFromJsonAsync<IReadOnlyList<LanguageResponse>>(
            "/api/v1/languages");
        Assert.NotNull(catalog);
        var english = Assert.Single(catalog, language => language.Code == "en");
        var polish = Assert.Single(catalog, language => language.Code == "pl");
        var lemma = $"private-{Guid.NewGuid():N}";

        var create = await SendWithAntiforgeryAsync(
            ownerClient,
            HttpMethod.Post,
            "/api/v1/vocabulary/private",
            new CreatePrivateVocabularyRequest(
                english.Id,
                lemma,
                "Noun",
                "A1",
                null,
                polish.Id,
                "definicja prywatna"));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var privateItem = await create.Content
            .ReadFromJsonAsync<UserVocabularyItemResponse>();
        Assert.NotNull(privateItem);

        var ownerSearch = await ownerClient.GetFromJsonAsync<VocabularySearchResponse>(
            $"/api/v1/vocabulary/search?languageId={english.Id}&query={lemma}");
        Assert.Single(Assert.IsType<VocabularySearchResponse>(ownerSearch).Items);

        var otherSearch = await otherClient.GetFromJsonAsync<VocabularySearchResponse>(
            $"/api/v1/vocabulary/search?languageId={english.Id}&query={lemma}");
        Assert.Empty(Assert.IsType<VocabularySearchResponse>(otherSearch).Items);

        var directDetails = await otherClient.GetAsync(
            $"/api/v1/vocabulary/lexemes/{privateItem.LexemeId}");
        Assert.Equal(HttpStatusCode.NotFound, directDetails.StatusCode);

        var directAdd = await SendWithAntiforgeryAsync(
            otherClient,
            HttpMethod.Post,
            "/api/v1/vocabulary/items",
            new AddVocabularyItemRequest(privateItem.LexemeSenseId));
        Assert.Equal(HttpStatusCode.NotFound, directAdd.StatusCode);

        var createPrivateList = await SendWithAntiforgeryAsync(
            ownerClient,
            HttpMethod.Post,
            "/api/v1/vocabulary/lists",
            new CreateVocabularyListRequest("Owner only", null));
        Assert.Equal(HttpStatusCode.Created, createPrivateList.StatusCode);
        var privateList = await createPrivateList.Content
            .ReadFromJsonAsync<VocabularyListDetailsResponse>();
        Assert.NotNull(privateList);

        var directListRead = await otherClient.GetAsync(
            $"/api/v1/vocabulary/lists/{privateList.Id}");
        Assert.Equal(HttpStatusCode.NotFound, directListRead.StatusCode);
    }

    private HttpClient CreateSecureClient() => factory.CreateClient(
        new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true,
        });

    private static async Task RegisterConfirmedAndLoginAsync(
        HttpClient client,
        string displayName,
        string password)
    {
        var email = $"learner-{Guid.NewGuid():N}@example.com";
        var registration = await SendWithAntiforgeryAsync(
            client,
            HttpMethod.Post,
            "/api/v1/auth/register",
            new RegisterRequest(email, password, displayName, "Europe/Warsaw"));
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        var developmentEmail = await client.GetFromJsonAsync<DevelopmentEmailResponse>(
            $"/api/v1/auth/development-emails/latest?email={Uri.EscapeDataString(email)}");
        Assert.NotNull(developmentEmail);
        var confirmationUri = new Uri(developmentEmail.ActionUrl);
        var confirmationQuery = QueryHelpers.ParseQuery(confirmationUri.Query);

        var confirmation = await SendWithAntiforgeryAsync(
            client,
            HttpMethod.Post,
            "/api/v1/auth/confirm-email",
            new ConfirmEmailRequest(
                Guid.Parse(confirmationQuery["userId"].ToString()),
                confirmationQuery["code"].ToString()));
        Assert.Equal(HttpStatusCode.OK, confirmation.StatusCode);

        var login = await SendWithAntiforgeryAsync(
            client,
            HttpMethod.Post,
            "/api/v1/auth/login",
            new LoginRequest(email, password, RememberMe: false));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    private static async Task<HttpResponseMessage> SendWithAntiforgeryAsync<TRequest>(
        HttpClient client,
        HttpMethod method,
        string requestUri,
        TRequest body)
    {
        var token = await client.GetFromJsonAsync<AntiforgeryTokenResponse>(
            "/api/v1/auth/antiforgery");
        Assert.NotNull(token);

        using var request = new HttpRequestMessage(method, requestUri)
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Add("X-XSRF-TOKEN", token.Token);
        return await client.SendAsync(request);
    }
}
