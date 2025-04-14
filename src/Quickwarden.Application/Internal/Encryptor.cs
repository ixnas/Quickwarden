using System.Security.Cryptography;

namespace Quickwarden.Application.Internal;

internal class Encryptor
{
    private readonly byte[] _secret;

    public Encryptor(byte[] secret)
    {
        _secret = secret;
    }

    public async Task<byte[]> Encrypt(byte[] bytes)
    {
        using var ms = new MemoryStream();
        using var aes = Aes.Create();
        aes.Key = _secret;
        
        ms.Write(aes.IV, 0, aes.IV.Length);
        await using (var cryptoStream =
                     new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cryptoStream.Write(bytes);

        }
        return ms.ToArray();
    }
}
