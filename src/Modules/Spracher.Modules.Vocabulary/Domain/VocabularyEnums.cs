namespace Spracher.Modules.Vocabulary.Domain;

public enum VocabularyVisibility
{
    Catalog = 0,
    Private = 1,
}

public enum VocabularySourceType
{
    Curated = 0,
    Import = 1,
    UserCreated = 2,
}

public enum PublicationStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2,
}

public enum UserVocabularyStatus
{
    New = 0,
    Learning = 1,
    Learned = 2,
    Suspended = 3,
}
