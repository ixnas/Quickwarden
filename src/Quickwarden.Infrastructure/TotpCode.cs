using OtpNet;
using Quickwarden.Application.PlugIns;
using Quickwarden.Application.PlugIns.Totp;

namespace Quickwarden.Infrastructure;

public class TotpCode : ITotpCode
{
    private readonly IClock _clock;
    private readonly Totp _totp;
    
    public string Code => _totp.ComputeTotp(_clock.UtcNow.DateTime);
    public int SecondsRemaining => _totp.RemainingSeconds(_clock.UtcNow.DateTime);

    public TotpCode(Totp totp, IClock clock)
    {
        _totp = totp;
        _clock = clock;
    }
}
