using System.Security.Cryptography;

namespace Quickwarden.Application.Internal;

internal class Decryptor
{
    private readonly byte[] _secret;

    public Decryptor(byte[] secret)
    {
        _secret = secret;
    }

    public async Task<byte[]> Decrypt(byte[] encrypted)
    {
        using var ms = new MemoryStream(encrypted);
        using var aes = Aes.Create();
        aes.Key = _secret;

        await using var outputStream = new MemoryStream();
        await using (var cryptoStream =
                     new CryptoStream(outputStream,
                                      aes.CreateDecryptor(),
                                      CryptoStreamMode.Write))
        {
            cryptoStream.Write(encrypted, 0, encrypted.Length);
        }

        return outputStream.ToArray()[16..];
    }
}
