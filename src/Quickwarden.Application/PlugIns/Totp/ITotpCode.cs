namespace Quickwarden.Application.PlugIns.Totp;

public interface ITotpCode
{
    string Code { get; }
    int SecondsRemaining { get; }
}
