namespace Quickwarden.Application.PlugIns.Bitwarden;

public interface IQuickwardenEnvironment
{
    Task Initialize();
    Task<bool> BitwardenCliInstalled();
}
