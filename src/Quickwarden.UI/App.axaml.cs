using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Quickwarden.UI.ViewModels;

namespace Quickwarden.UI;

public partial class App : Avalonia.Application
{
    private TrayIcon? _trayIcon;
    private AppViewModel? _appViewModel;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            _appViewModel = new AppViewModel();
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            DataContext = _appViewModel;
            var trayIcon = OperatingSystem.IsMacOS() ? new Uri("avares://Quickwarden.UI/Assets/Icon-trayicon-template-256.png") : new Uri("avares://Quickwarden.UI/Assets/Icon-sm-256.png");
            _trayIcon = new TrayIcon()
            {
                Command = _appViewModel.ShowWindowCommand,
                Icon = new WindowIcon(new Bitmap(AssetLoader.Open(trayIcon))),
                Menu = [
                    new NativeMenuItem("Show")
                    {
                        Command = _appViewModel.ShowWindowCommand
                    },
                    new NativeMenuItem("Exit")
                    {
                        Command = _appViewModel.ExitCommand
                    }
                ]
            };
            MacOSProperties.SetIsTemplateIcon(_trayIcon, true);
        }
        
        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}