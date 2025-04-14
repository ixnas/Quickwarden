using Quickwarden.Application.PlugIns;

namespace Quickwarden.Tests.Fakes;

public class StaticClockFake : IClock
{
    public DateTimeOffset UtcNow => new DateTime(2025, 3, 10, 0, 0, 0, DateTimeKind.Utc);
}
