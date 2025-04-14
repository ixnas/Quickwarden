using System.ComponentModel;
using Quickwarden.Application.PlugIns.Bitwarden;
using Quickwarden.Infrastructure.Internal;

namespace Quickwarden.Infrastructure;

public class QuickwardenEnvironment : IQuickwardenEnvironment
{
    private readonly static string AppDataPath =
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Quickwarden");
    public static string ConfigPath { get; } = Path.Join(AppDataPath, "config.bin");
    public static string VaultsPath { get; } = Path.Join(AppDataPath, "vaults");

    public Task Initialize()
    {
        Directory.CreateDirectory(AppDataPath);
        Directory.CreateDirectory(VaultsPath);
        return Task.CompletedTask;
    }

    public async Task<bool> BitwardenCliInstalled()
    {
        try
        {
            var result = await ShellExecutor.ExecuteAsync("bw", ["sdk-version"], []);
            return result.StdOutLines.Length == 1;
        }
        catch (Win32Exception)
        {
            return false;
        }
    }
}
