using Quickwarden.Application.PlugIns;

namespace Quickwarden.Infrastructure;

public static class SecretRepositoryFactory
{
    public static IEncryptionManager Create()
    {
        if (OperatingSystem.IsWindows())
            return new WindowsHelloEncryptionManager();
        return new MultiPlatformEncryptionManager();
    }
}