namespace Quickwarden.Application;

public enum ApplicationInitializeResult
{
    Success,
    CouldntAccessKeychain,
    BitwardenCliNotFound
}
