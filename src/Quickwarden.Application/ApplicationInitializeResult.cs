namespace Quickwarden.Application;

public enum ApplicationInitializeResult
{
    Success,
    CouldntWriteToKeychain,
    BitwardenCliNotFound
}
