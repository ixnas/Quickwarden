#if WINDOWS
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
        var credentialKey = await GetCredentialKey();
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

    private static async Task<string> GetCredentialKey()
    {
        var publicKeyTokenString = await GetCertificateKey();
        var userSid = WindowsIdentity.GetCurrent().User?.Value ?? "no-user";
        var keyMaterial = $"Quickwarden-{publicKeyTokenString}-{userSid}";
        var keyMaterialBytes = Encoding.UTF8.GetBytes(keyMaterial);
        var hash = SHA256.HashData(keyMaterialBytes);
        return Convert.ToHexString(hash);
    }

    private static string ToApplicationSecret(KeyCredential credential)
    {
        var byteArray = credential.RetrievePublicKey().ToArray();
        var shaByteArray = SHA256.HashData(byteArray);
        return Convert.ToHexString(shaByteArray);
    }

    private static async Task<string> GetCertificateKey()
    {
        if (string.IsNullOrWhiteSpace(Environment.ProcessPath))
            return string.Empty;
        try
        {
            var executingCert = X509Certificate.CreateFromSignedFile(Environment.ProcessPath);
            if (executingCert == null)
                return string.Empty;
            var assemblyKey = executingCert.GetPublicKey();
            var keyHash = SHA256.HashData(assemblyKey);

            return Convert.ToBase64String(keyHash);
        }
        catch (CryptographicException)
        {
            return string.Empty;
        }
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