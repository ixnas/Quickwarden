namespace Quickwarden.Application.Internal;

internal record Account()
{
    public required string Id { get; init; }
    public required string Username { get; init; }
    public required string Secret { get; init; }
}
