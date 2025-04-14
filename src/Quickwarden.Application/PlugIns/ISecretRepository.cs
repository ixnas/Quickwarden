namespace Quickwarden.Application.PlugIns;

public interface ISecretRepository
{
    Task<bool> Store(string secret);
    Task<string?> Get();
}
