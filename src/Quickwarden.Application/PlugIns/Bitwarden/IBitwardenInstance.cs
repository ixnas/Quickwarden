namespace Quickwarden.Application.PlugIns.Bitwarden;

public interface IBitwardenInstance
{
    string Id { get; }
    string Username { get; }
    Task<BitwardenVaultItem[]> GetVaultItems(CancellationToken cancellationToken);
}
