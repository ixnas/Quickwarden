namespace Quickwarden.Application.PlugIns.Bitwarden;

public interface IBinaryConfigurationRepository
{
    Task Store(byte[] configuration);
    Task<byte[]> Get();
}
