namespace Quickwarden.Infrastructure.Internal;

[Serializable]
internal class BitwardenVaultEntryLogin
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Totp { get; set; }
}
