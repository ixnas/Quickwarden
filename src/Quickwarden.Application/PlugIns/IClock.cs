namespace Quickwarden.Application.PlugIns;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
