namespace Quickwarden.Application.PlugIns;

public interface ISecretRepository
{
    Task<string?> Get();
}
