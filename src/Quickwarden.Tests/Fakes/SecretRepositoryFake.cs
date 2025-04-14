using Quickwarden.Application.PlugIns;

namespace Quickwarden.Tests.Fakes;

public class SecretRepositoryFake : ISecretRepository
{
    public bool CanStore { get; set; } = true;
    public bool CanGet { get; set; } = true;
    public string? Secret { get; set; }

    public Task<bool> Store(string secret)
    {
        if (!CanStore)
            return Task.FromResult(false);

        Secret = secret;
        return Task.FromResult(true);
    }

    public Task<string?> Get()
    {
        if (!CanGet)
            return Task.FromResult<string?>(null);

        return Task.FromResult(Secret);
    }
}
