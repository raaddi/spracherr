namespace Spracher.Api.IntegrationTests.Infrastructure;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IntegrationFactAttribute : FactAttribute
{
    private const string IntegrationTestsVariable = "RUN_INTEGRATION_TESTS";

    public IntegrationFactAttribute()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable(IntegrationTestsVariable),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            Skip = $"Set {IntegrationTestsVariable}=true and start Docker to run this test.";
        }
    }
}
