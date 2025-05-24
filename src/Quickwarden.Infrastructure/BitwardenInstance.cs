using System.Text.Json;
using System.Text.Json.Serialization;
using Quickwarden.Application.PlugIns.Bitwarden;
using Quickwarden.Infrastructure.Internal;

namespace Quickwarden.Infrastructure;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(BitwardenVaultEntry))]
[JsonSerializable(typeof(BitwardenVaultEntry[]))]
internal partial class InfrastructureJsonSerializerContext : JsonSerializerContext
{
}

public class BitwardenInstance : IBitwardenInstance
{
    private readonly BitwardenInstanceKey _key;
    public string Id => _key.Id;
    public string Username => _key.Username;

    public BitwardenInstance(BitwardenInstanceKey key)
    {
        _key = key;
    }

    public async Task<BitwardenVaultItem[]> GetVaultItems(CancellationToken cancellationToken)
    {
        var command = "bw";
        var vaultPath = Path.Join(QuickwardenEnvironment.VaultsPath, _key.Id);
        var env = new Dictionary<string, string>()
        {
            ["BITWARDENCLI_APPDATA_DIR"] = vaultPath,
            ["BW_NOINTERACTION"] = "true",
            ["BW_SESSION"] = _key.Secret,
        };
        await Sync(command, env);
        return await GetVaultItems(command, env);
    }

    private async static Task Sync(string command, Dictionary<string, string> env)
    {
        string[] args = ["sync"];
        await ShellExecutor.ExecuteAsync(command, args, env);
    }

    private async Task<BitwardenVaultItem[]> GetVaultItems(string command, Dictionary<string, string> env)
    {
        string[] args = ["list", "items"];
        var result = await ShellExecutor.ExecuteAsync(command, args, env);
        var entries = JsonSerializer.Deserialize<BitwardenVaultEntry[]>(result.StdOutLines.Single(),
                                                                        new JsonSerializerOptions
                                                                        {
                                                                            TypeInfoResolver = InfrastructureJsonSerializerContext.Default,
                                                                            PropertyNameCaseInsensitive = true
                                                                        });
        if (entries == null)
            throw new InvalidOperationException();
        return entries.Select(entry => new BitwardenVaultItem
        {
            Id = entry.Id,
            Name = entry.Name,
            Username = entry.Login?.Username,
            Password = entry.Login?.Password,
            Totp = entry.Login?.Totp,
            Notes = entry.Notes,
            VaultId = Id,
        }).ToArray();
    }
}
