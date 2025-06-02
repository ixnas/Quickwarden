using System.Security.Cryptography;
using KeySharp;
using Quickwarden.Application.PlugIns;

namespace Quickwarden.Infrastructure;

public class AesEncryption
{
    private readonly Aes _aes;

    public AesEncryption(string secret)
    {
        var secretBytes = Convert.FromHexString(secret);
        _aes = Aes.Create();
        _aes.Key = secretBytes;
    }

    public AesEncryption(Aes aes)
    {
        _aes = aes;
    }
    
    public async Task<byte[]> Encrypt(byte[] bytes)
    {
        using var ms = new MemoryStream();
        
        ms.Write(_aes.IV, 0, _aes.IV.Length);
        await using (var cryptoStream =
                     new CryptoStream(ms, _aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cryptoStream.Write(bytes);

        }
        return ms.ToArray();
    }
    
    public async Task<byte[]> Decrypt(byte[] encrypted)
    {
        using var ms = new MemoryStream(encrypted);

        await using var outputStream = new MemoryStream();
        await using (var cryptoStream =
                     new CryptoStream(outputStream,
                         _aes.CreateDecryptor(),
                         CryptoStreamMode.Write))
        {
            cryptoStream.Write(encrypted, 0, encrypted.Length);
        }

        return outputStream.ToArray()[16..];
    }
}

public class MultiPlatformEncryptionManager : IEncryptionManager
{
    private const string Package = "Quickwarden";
    private const string Service = "Quickwarden";
    private const string User = "config-encryption-key";
    private AesEncryption? _encryption;

    public Task<bool> Initialize()
    {
        return Task.Run(() =>
        {
            try
            {
                var storedSecret = Keyring.GetPassword(Package, Service, User);
                if (storedSecret == null)
                    return false;
                _encryption = new AesEncryption(storedSecret);
                return true;
            }
            catch
            {
                try
                {
                    var newSecret = GenerateSecret();
                    Keyring.SetPassword(Package,
                        Service,
                        User,
                        newSecret);
                    _encryption = new AesEncryption(newSecret);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        });
    }

    public async Task<byte[]> Encrypt(byte[] data)
    {
        return await _encryption!.Encrypt(data);
    }

    public async Task<byte[]> Decrypt(byte[] data)
    {
        return await _encryption!.Decrypt(data);
    }

    private static string GenerateSecret()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToHexString(bytes);
    }
}
