using KeySharp;
using Quickwarden.Application.PlugIns;

namespace Quickwarden.Infrastructure;

public class MultiPlatformSecretRepository : ISecretRepository
{
    private const string Package = "Quickwarden";
    private const string Service = "Quickwarden";
    private const string User = "config-encryption-key";

    public Task<string?> Get()
    {
        return Task.Run(() =>
        {
            try
            {
                return Keyring.GetPassword(Package, Service, User);
            }
            catch
            {
                try
                {
                    var secret = GenerateSecret();
                    Keyring.SetPassword(Package,
                        Service,
                        User,
                        secret);
                    return secret;
                }
                catch
                {
                    return null;
                }
            }
        });
    }

    private static string GenerateSecret()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToHexString(bytes);
    }
}
