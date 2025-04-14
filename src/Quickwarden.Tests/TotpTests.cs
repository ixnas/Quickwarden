using Quickwarden.Infrastructure;
using Quickwarden.Tests.Fakes;

namespace Quickwarden.Tests;

public class TotpTests
{
    [Fact]
    public void ParsesBase32()
    {
        const string expectedKey = "076986";
        var totp = new TotpGenerator(new StaticClockFake());
        var key = totp.GenerateFromSecret("W5WQ W3P4 3M3I 2A6M 4SSD 4SM2 SJT6 OZZH 3ASR ZURK 24JR AYU5 WSKA");
        Assert.Equal(expectedKey, key.Code);
    }

    [Fact]
    public void ParsesUrI()
    {
        const string expectedKey = "113139";
        var totp = new TotpGenerator(new StaticClockFake());
        var key = totp.GenerateFromSecret("otpauth://totp/Someplace:someaccount?secret=2KSSJX2XNU2UK3N5RA5JHLGKXCELCY7N&issuer=Someplace");
        Assert.Equal(expectedKey, key.Code);
    }
}