using Quickwarden.Application.PlugIns;

namespace Quickwarden.Infrastructure;

public class DefaultClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
