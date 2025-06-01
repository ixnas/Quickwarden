#if WINDOWS
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Windows.Security.Credentials;
using Quickwarden.Application.PlugIns;

namespace Quickwarden.Infrastructure;

public class WindowsHelloSecretRepository : ISecretRepository
{
    public async Task<string?> Get()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException();
        var isSupported = await KeyCredentialManager.IsSupportedAsync();
        if (!isSupported)
            return null;
        var credentialKey = GetCredentialKey();
        var previousSecret = await KeyCredentialManager.OpenAsync(credentialKey);
        if (previousSecret.Status == KeyCredentialStatus.NotFound)
        {
            var createResult =
                await KeyCredentialManager.RequestCreateAsync(credentialKey, KeyCredentialCreationOption.FailIfExists);
            if (createResult.Status != KeyCredentialStatus.Success)
                return null;
            return ToApplicationSecret(createResult.Credential);
        }

        if (previousSecret.Status != KeyCredentialStatus.Success)
            return null;
        return ToApplicationSecret(previousSecret.Credential);
    }

    private static string GetCredentialKey()
    {
        var publicKeyToken = Assembly.GetExecutingAssembly().GetName().GetPublicKeyToken() ?? [];
        var publicKeyTokenString = Convert.ToHexString(publicKeyToken);
        var userSid = WindowsIdentity.GetCurrent().User?.Value ?? "no-user";
        var keyMaterial = $"Quickwarden-{publicKeyTokenString}-{userSid}";
        var keyMaterialBytes = Encoding.UTF8.GetBytes(keyMaterial);
        var hash = SHA256.HashData(keyMaterialBytes);
        return Convert.ToBase64String(hash);
    }

    private static string ToApplicationSecret(KeyCredential credential)
    {
        var byteArray = credential.RetrievePublicKey().ToArray();
        var shaByteArray = SHA256.HashData(byteArray);
        return Convert.ToHexString(shaByteArray);
    }
}
#else
using Quickwarden.Application.PlugIns;

namespace Quickwarden.Infrastructure;

public class WindowsHelloSecretRepository : ISecretRepository
{
    public Task<string?> Get()
    {
        throw new PlatformNotSupportedException();
    }
}
#endif