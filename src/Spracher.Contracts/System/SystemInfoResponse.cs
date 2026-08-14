namespace Spracher.Contracts.System;

public sealed record SystemInfoResponse(
    string Name,
    string ApiVersion,
    string Environment,
    DateTimeOffset ServerTimeUtc);
