using Quickwarden.Application.PlugIns;

namespace Quickwarden.Infrastructure;

public static class SecretRepositoryFactory
{
    public static ISecretRepository Create()
    {
        if (OperatingSystem.IsWindows())
            return new WindowsHelloSecretRepository();
        return new MultiPlatformSecretRepository();
    }
}