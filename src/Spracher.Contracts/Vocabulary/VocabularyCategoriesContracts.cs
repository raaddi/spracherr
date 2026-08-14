namespace Spracher.Contracts.Vocabulary;

public sealed record CreateVocabularyCategoryRequest(string Name, string Color);

public sealed record AssignVocabularyCategoriesRequest(IReadOnlyList<Guid> CategoryIds);

public sealed record VocabularyCategoriesResponse(
    IReadOnlyList<VocabularyCategoryResponse> Items);

public sealed record VocabularyCategoryResponse(
    Guid Id,
    string Name,
    string Color,
    IReadOnlyList<Guid> AssignedUserVocabularyItemIds);

public sealed record UserVocabularyCategoriesResponse(
    Guid UserVocabularyItemId,
    IReadOnlyList<Guid> CategoryIds);

public sealed record VocabularyMutationResponse(string Message);
