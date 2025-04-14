namespace Quickwarden.Application;

public record SearchResultItem
{
    public required string Name { get; init; }
    public required string Username { get; init; }
    public required string Id { get; init; }
    public required bool HasTotp { get; init; }
    public required bool HasPassword { get; init; }
    public required bool HasUsername { get; init; }
}
