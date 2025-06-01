using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Quickwarden.Application;
using Quickwarden.Infrastructure;
using Quickwarden.UI.Internal;
using Quickwarden.UI.Views;
using ApplicationController = Quickwarden.Application.ApplicationController;

namespace Quickwarden.UI.ViewModels;

public partial class AppViewModel : ViewModelBase
{
    private readonly ApplicationController _application;
    private readonly MainWindow _mainWindow;
    private readonly GlobalKeyboardShortcutManager _globalKeyboardShortcutManager;

    public AppViewModel()
    {
        // var fixture = new ApplicationFixture();
        // _application = new ApplicationController(fixture.SecretRepository,
        //                                          fixture.BitwardenInstanceRepository,
        //                                          fixture.BinaryConfigurationRepository,
        //                                          new TotpGenerator(fixture.StaticClock),
        //                                          new QuickwardenEnvironmentFake());
        _application = new ApplicationController(SecretRepositoryFactory.Create(),
                                                 new BitwardenInstanceRepository(),
                                                 new BinaryConfigurationRepository(),
                                                 new TotpGenerator(new DefaultClock()),
                                                 new QuickwardenEnvironment());
        _globalKeyboardShortcutManager =
            new GlobalKeyboardShortcutManager(ShowWindow);
        _mainWindow = new MainWindow();
        _mainWindow.Activated += (_, _) => _mainWindow.SearchBox.Focus();
        _mainWindow.DataContext = new MainWindowViewModel(_mainWindow);
        _mainWindow.InitializeComponent();
        _globalKeyboardShortcutManager.StartListening();
        _mainWindow.Show();
        _mainWindow.Hide();
        Task.Run(Initialize);
    }

    [RelayCommand]
    private void ShowWindow()
    {
        _mainWindow.Show();
        _mainWindow.BringIntoView();
        _mainWindow.Activate();
        _mainWindow.Focus();
    }

    [RelayCommand]
    private void Exit()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
            application)
        {
            _globalKeyboardShortcutManager.Dispose();
            application.Shutdown();
        }
    }

    private async Task Initialize()
    {
        try
        {
            var result = await _application.Initialize();
            if (result == ApplicationInitializeResult.BitwardenCliNotFound)
            {
                await
                    FatalMessageBox("Bitwarden CLI was not found.\r\nMake sure it is installed and that its installation directory is included in your PATH environment variable.");
                return;
            }

            if (result == ApplicationInitializeResult.CouldntAccessKeychain)
            {
                if (OperatingSystem.IsMacOS())
                    await FatalMessageBox("Could not access your macOS Keychain.");
                else
                    await FatalMessageBox("Windows Hello authentication failed.");
                return;
            }

            // Initialize viewmodel
            Dispatcher.UIThread.Invoke(() => ((MainWindowViewModel)_mainWindow.DataContext)
                                           .SetApplicationController(_application));
        }
        catch (Exception ex)
        {
            await FatalMessageBox($"{ex.GetType().Name}: {ex.Message}\r\n\r\n${ex.StackTrace}");
        }
    }

    private async Task FatalMessageBox(string message)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", message, ButtonEnum.Ok, Icon.Error);
            return await messageBox.ShowAsync();
        });
        await Dispatcher.UIThread.InvokeAsync(Exit);
    }
}
