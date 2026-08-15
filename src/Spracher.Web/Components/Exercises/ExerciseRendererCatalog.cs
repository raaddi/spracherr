namespace Spracher.Web.Components.Exercises;

public sealed class ExerciseRendererCatalog
{
    private readonly Dictionary<string, Type> _renderers =
        new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            ["multiple-choice"] = typeof(MultipleChoiceExerciseRenderer),
            ["fill-in-blank"] = typeof(FillInBlankExerciseRenderer),
        };

    public bool TryGet(string typeKey, out Type? componentType) =>
        _renderers.TryGetValue(typeKey, out componentType);
}
