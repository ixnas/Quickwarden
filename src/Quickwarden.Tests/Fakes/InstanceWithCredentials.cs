using Quickwarden.Application.PlugIns.Bitwarden;

namespace Quickwarden.Tests.Fakes;

public record InstanceWithCredentials(
    string Username,
    string Password,
    string Totp,
    IBitwardenInstance Instance,
    BitwardenInstanceKey Key);
