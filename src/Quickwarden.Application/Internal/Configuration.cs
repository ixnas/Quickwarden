namespace Quickwarden.Application.Internal;

internal record Configuration()
{
    public int Version { get; init; }
    public Account[] Accounts { get; init; } = [];
}