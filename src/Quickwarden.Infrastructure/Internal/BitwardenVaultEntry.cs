namespace Quickwarden.Infrastructure.Internal;

[Serializable]
internal class BitwardenVaultEntry
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Notes { get; set; }
    public BitwardenVaultEntryLogin? Login { get; set; }
}
