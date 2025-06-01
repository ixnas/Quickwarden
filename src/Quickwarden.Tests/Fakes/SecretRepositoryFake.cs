using Quickwarden.Application.PlugIns;

namespace Quickwarden.Tests.Fakes;

public class SecretRepositoryFake : ISecretRepository
{
    public bool CanGet { get; set; } = true;
    private readonly string? _secret = GenerateSecret();

    public Task<string?> Get()
    {
        if (!CanGet)
            return Task.FromResult<string?>(null);

        return Task.FromResult(_secret);
    }

    private static string GenerateSecret()
    {
        var bytes = new byte[32];
        for (var i = 0; i < 32; i++)
        {
            bytes[i] = 0xff;
        }
        return Convert.ToHexString(bytes);
    }
}
