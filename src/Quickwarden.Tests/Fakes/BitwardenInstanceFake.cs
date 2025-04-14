using Quickwarden.Application.PlugIns.Bitwarden;

namespace Quickwarden.Tests.Fakes;

internal class BitwardenInstanceFake : IBitwardenInstance
{
    public string Id { get; set; }
    public string Username { get; set; }
    public List<BitwardenVaultItem> VaultItems { get; set; } = [];

    public Task<BitwardenVaultItem[]> GetVaultItems(CancellationToken cancellationToken)
    {
        return Task.FromResult(VaultItems.ToArray());
    }
}
