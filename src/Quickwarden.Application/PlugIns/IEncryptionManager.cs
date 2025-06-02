namespace Quickwarden.Application.PlugIns;

public interface IEncryptionManager
{
    Task<bool> Initialize();
    Task<byte[]> Encrypt(byte[] data);
    Task<byte[]> Decrypt(byte[] data);
}
