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
        var certificateKey = GetCertificateKey();
        var credentialKey = GetCredentialKey(certificateKey);
        var previousSecret = await KeyCredentialManager.OpenAsync(credentialKey);
        if (previousSecret.Status == KeyCredentialStatus.NotFound)
        {
            var createResult =
                await KeyCredentialManager.RequestCreateAsync(credentialKey, KeyCredentialCreationOption.FailIfExists);
            if (createResult.Status != KeyCredentialStatus.Success)
                return null;
            return await ToApplicationSecret(createResult.Credential, certificateKey);
        }

        if (previousSecret.Status != KeyCredentialStatus.Success)
            return null;
        return await ToApplicationSecret(previousSecret.Credential, certificateKey);
    }

    private string GetCredentialKey(byte[] certificateKey)
    {
        var publicKeyTokenString = Convert.ToBase64String(certificateKey);
        var userSid = WindowsIdentity.GetCurrent().User?.Value ?? "no-user";
        var keyMaterial = $"Quickwarden-{publicKeyTokenString}-{userSid}";
        var keyMaterialBytes = Encoding.UTF8.GetBytes(keyMaterial);
        var hash = SHA256.HashData(keyMaterialBytes);
        return Convert.ToHexString(hash);
    }

    private static async Task<string?> ToApplicationSecret(KeyCredential credential, byte[] certificateKey)
    {
        var signResult = await credential.RequestSignAsync(certificateKey.AsBuffer());
        if (signResult.Status != KeyCredentialStatus.Success)
            return null;
        var signed = signResult.Result.ToArray();
        var shaByteArray = SHA256.HashData(signed);
        return Convert.ToHexString(shaByteArray);
    }

    private static byte[] GetCertificateKey()
    {
        if (string.IsNullOrWhiteSpace(Environment.ProcessPath))
            return [];
        try
        {
            var executingCert = X509Certificate.CreateFromSignedFile(Environment.ProcessPath);
            if (executingCert == null)
                return [];
            var assemblyKey = executingCert.GetPublicKey();
            var keyHash = SHA256.HashData(assemblyKey);

            return keyHash;
        }
        catch (CryptographicException)
        {
            return [];
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