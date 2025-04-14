namespace Quickwarden.Application.PlugIns.Bitwarden;

public class BitwardenVaultItem
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? Totp { get; init; }
    public string? Notes { get; init; }
}
