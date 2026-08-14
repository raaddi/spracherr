namespace Spracher.Modules.Exercises.Engine;

internal sealed class ExerciseTypeRegistry
{
    private readonly Dictionary<string, IExerciseTypeHandler> _handlers;

    public ExerciseTypeRegistry(IEnumerable<IExerciseTypeHandler> handlers)
    {
        _handlers = handlers.ToDictionary(handler => handler.TypeKey, StringComparer.Ordinal);
    }

    public IExerciseTypeHandler GetRequired(string typeKey, int schemaVersion)
    {
        if (!_handlers.TryGetValue(typeKey, out var handler)
            || handler.SchemaVersion != schemaVersion)
        {
            throw new InvalidOperationException(
                $"No exercise handler is registered for '{typeKey}' schema {schemaVersion}.");
        }

        return handler;
    }
}
