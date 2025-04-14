namespace Quickwarden.Application.PlugIns.FrontEnd;

public enum SignInResult
{
    AlreadySignedIn,
    WrongCredentials,
    Success,
    Timeout,
    Missing2Fa,
}
