using Quickwarden.Application.PlugIns.Bitwarden;

namespace Quickwarden.Tests.Fakes;

public class BinaryConfigurationRepositoryFake : IBinaryConfigurationRepository
{
    private byte[] _configuration = [];

    public Task Store(byte[] configuration)
    {
        _configuration = configuration;
        return Task.CompletedTask;
    }

    public Task<byte[]> Get()
    {
        return Task.FromResult(_configuration);
    }
}
