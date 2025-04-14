using KeySharp;
using Quickwarden.Application.PlugIns;

namespace Quickwarden.Infrastructure;

public class SecretRepository : ISecretRepository
{
    private const string Package = "Quickwarden";
    private const string Service = "Quickwarden";
    private const string User = "config-encryption-key";
    public Task<bool> Store(string secret)
    {
        return Task.Run(() =>
        {
            try
            {
                Keyring.SetPassword(Package,
                                    Service,
                                    User,
                                    secret);
                return true;
            }
            catch
            {
                return false;
            }
        });
    }

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
                return null;
            }
        });
    }
}
