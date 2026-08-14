namespace Spracher.BuildingBlocks.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
