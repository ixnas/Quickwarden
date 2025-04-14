using Quickwarden.Application.PlugIns.Bitwarden;

namespace Quickwarden.Infrastructure;

public class BinaryConfigurationRepository : IBinaryConfigurationRepository
{
    public async Task Store(byte[] configuration)
    {
        await File.WriteAllBytesAsync(QuickwardenEnvironment.ConfigPath, configuration);
    }

    public async Task<byte[]> Get()
    {
        if (!File.Exists(QuickwardenEnvironment.ConfigPath))
            return [];

        return await File.ReadAllBytesAsync(QuickwardenEnvironment.ConfigPath);
    }
}
