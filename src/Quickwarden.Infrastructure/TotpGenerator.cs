using OtpNet;
using Quickwarden.Application.PlugIns;
using Quickwarden.Application.PlugIns.Totp;

namespace Quickwarden.Infrastructure;

public class TotpGenerator : ITotpGenerator
{
    private readonly IClock _clock;

    public TotpGenerator(IClock clock)
    {
        _clock = clock;
    }

    public ITotpCode GenerateFromSecret(string secret)
    {
        if (secret.StartsWith("otpauth://totp/"))
        {
            const string secretParameter = "secret=";
            var secretIndex = secret.IndexOf(secretParameter, StringComparison.InvariantCultureIgnoreCase) + secretParameter.Length;
            var endIndex = secret.Substring(secretIndex).IndexOf('&');
            secret = secret.Substring(secretIndex, endIndex);
        }
        var secretBytes = Base32Encoding.ToBytes(secret.Replace(" ", "").ToUpperInvariant());
        var totp = new Totp(secretBytes);
        return new TotpCode(totp, _clock);
    }
}