namespace LearnSql.Tests.Support;

public sealed class SingleDeterministicTimeProvider : TimeProvider
{
    public static readonly DateTimeOffset DefaultInstant = new DateTimeOffset(
        2024,
        3,
        15,
        10,
        20,
        30,
        123,
        TimeSpan.Zero
    );

    private readonly DateTimeOffset _instant;
    private readonly TimeZoneInfo _localTimeZone;

    public SingleDeterministicTimeProvider()
        : this(DefaultInstant) { }

    public SingleDeterministicTimeProvider(DateTimeOffset instant)
        : this(instant, TimeZoneInfo.Utc) { }

    public SingleDeterministicTimeProvider(DateTimeOffset instant, TimeZoneInfo localTimeZone)
    {
        _instant = instant;
        _localTimeZone = localTimeZone;
    }

    public override TimeZoneInfo LocalTimeZone => _localTimeZone;

    public override DateTimeOffset GetUtcNow() => _instant;
}
