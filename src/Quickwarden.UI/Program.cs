using System;
using System.Threading;
using Avalonia;

namespace Quickwarden.UI;

sealed class Program
{
    private static Mutex mutex = new Mutex(true, "quickwarden-924e10d2-a114-42ca-bb1d-1996cc0c72a7");
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (!mutex.WaitOne(TimeSpan.Zero, true)) return;
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        mutex.ReleaseMutex();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .With(new MacOSPlatformOptions()
            {
                ShowInDock = false
            });
}