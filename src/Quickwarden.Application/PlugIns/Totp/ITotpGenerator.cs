namespace Quickwarden.Application.PlugIns.Totp;

public interface ITotpGenerator
{
    ITotpCode GenerateFromSecret(string secret);
}