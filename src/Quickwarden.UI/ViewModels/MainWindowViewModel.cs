using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Quickwarden.Application;
using Quickwarden.UI.Views;

namespace Quickwarden.UI.ViewModels;

public record CredentialListItem : SearchResultItem
{
    public override string ToString()
    {
        return $"{Name}\r\n{Username}";
    }
}

public partial class MainWindowViewModel : ViewModelBase
{
    private ApplicationController? _applicationController;
    private readonly MainWindow _mainWindow;
    private SettingsWindow? _settingsWindow;
    private CredentialListItem[] _credentials = [];
    private CredentialListItem? _selectedCredential;
    private string _searchBoxQuery = string.Empty;
    private bool _isSyncing;
    public string SettingsShortcut => OperatingSystem.IsMacOS() ? "⌘S" : "Ctrl-S";
    public KeyGesture CopyUsernameGesture => OperatingSystem.IsMacOS()
                                                 ? new KeyGesture(Key.U, KeyModifiers.Meta)
                                                 : new KeyGesture(Key.U, KeyModifiers.Control);
    public string SyncShortcut => OperatingSystem.IsMacOS() ? "⌘R" : "Ctrl-R";
    public KeyGesture SyncGesture => OperatingSystem.IsMacOS()
                                                 ? new KeyGesture(Key.R, KeyModifiers.Meta)
                                                 : new KeyGesture(Key.R, KeyModifiers.Control);
    public string CopyUsernameShortcut => OperatingSystem.IsMacOS() ? "⌘U" : "Ctrl-U";
    public KeyGesture CopyPasswordGesture => OperatingSystem.IsMacOS()
                                                 ? new KeyGesture(Key.P, KeyModifiers.Meta)
                                                 : new KeyGesture(Key.P, KeyModifiers.Control);
    public string CopyPasswordShortcut => OperatingSystem.IsMacOS() ? "⌘P" : "Ctrl-P";
    public KeyGesture Copy2FaGesture => OperatingSystem.IsMacOS()
                                            ? new KeyGesture(Key.T, KeyModifiers.Meta)
                                            : new KeyGesture(Key.T, KeyModifiers.Control);
    public string Copy2FaShortcut => OperatingSystem.IsMacOS() ? "⌘T" : "Ctrl-T";
    public KeyGesture SettingsShortcutGesture => OperatingSystem.IsMacOS()
                                                     ? new KeyGesture(Key.S, KeyModifiers.Meta)
                                                     : new KeyGesture(Key.S, KeyModifiers.Control);
    public bool ApplicationInitialized => _applicationController != null;
    public string SyncShortcutLabel => IsSyncing ? " Syncing..." : " Sync";
    public string SyncLabelColor => IsSyncing ? "#888" : "#000";
    public string SearchBoxWatermark => ApplicationInitialized ? "Search..." : "Loading...";
    public bool CopyUsernameEnabled => SelectedCredential?.HasUsername == true;
    public bool CopyPasswordEnabled => SelectedCredential?.HasPassword == true;
    public bool Copy2FaEnabled => SelectedCredential?.HasTotp == true;
    public string SearchBoxQuery
    {
        get => _searchBoxQuery;
        set
        {
            _searchBoxQuery = value;
            OnPropertyChanged();
            Credentials = _applicationController
                          .Search(value)
                          .Select(val => new CredentialListItem()
                          {
                              Name = val.Name,
                              Username = val.Username,
                              Id = val.Id,
                              HasTotp = val.HasTotp,
                              HasPassword = val.HasPassword,
                              HasUsername = val.HasUsername,
                          })
                          .ToArray();
        }
    }
    public CredentialListItem[] Credentials
    {
        get => _credentials;
        set
        {
            _credentials = value;
            OnPropertyChanged();
            SelectedCredential = value.Length > 0 ? value[0] : null;
        }
    }
    public CredentialListItem? SelectedCredential
    {
        get => _selectedCredential;
        set
        {
            _selectedCredential = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CopyUsernameEnabled));
            OnPropertyChanged(nameof(CopyPasswordEnabled));
            OnPropertyChanged(nameof(Copy2FaEnabled));
        }
    }
    private bool IsSyncing
    {
        get => _isSyncing;
        set
        {
            _isSyncing = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SyncShortcutLabel));
            OnPropertyChanged(nameof(SyncLabelColor));
        }
    }

    public MainWindowViewModel(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    [RelayCommand]
    public void KeyUp()
    {
        if (Credentials.Length < 1)
            return;
        var selectedCredentialIndex = Credentials.ToList().IndexOf(SelectedCredential);
        if (selectedCredentialIndex > 0)
        {
            SelectedCredential = Credentials[selectedCredentialIndex - 1];
        }
    }

    [RelayCommand]
    public void KeyDown()
    {
        if (Credentials.Length < 1)
            return;
        var selectedCredentialIndex = Credentials.ToList().IndexOf(SelectedCredential);
        if (selectedCredentialIndex < Credentials.Length - 1)
        {
            SelectedCredential = Credentials[selectedCredentialIndex + 1];
        }
    }

    [RelayCommand]
    public async Task CopyUsername()
    {
        if (SelectedCredential == null || !SelectedCredential.HasUsername || _applicationController == null)
            return;
        await _mainWindow.Clipboard.SetTextAsync(await _applicationController.GetUsername(SelectedCredential.Id));
        Hide();
    }

    [RelayCommand]
    public async Task CopyPassword()
    {
        if (SelectedCredential == null || !SelectedCredential.HasPassword || _applicationController == null)
            return;
        await _mainWindow.Clipboard.SetTextAsync(await _applicationController.GetPassword(SelectedCredential.Id));
        Hide();
    }

    [RelayCommand]
    public async Task Copy2Fa()
    {
        if (SelectedCredential == null || !SelectedCredential.HasTotp || _applicationController == null)
            return;
        await _mainWindow.Clipboard.SetTextAsync((await _applicationController.GetTotp(SelectedCredential.Id)).Code);
        Hide();
    }

    public void SetApplicationController(ApplicationController applicationController)
    {
        _applicationController = applicationController;
        OnPropertyChanged(nameof(ApplicationInitialized));
        OnPropertyChanged(nameof(SearchBoxWatermark));
        SearchBoxQuery = string.Empty;
        _mainWindow.SearchBox.Focus();
    }

    [RelayCommand]
    public void Hide()
    {
        _mainWindow.Hide();
        if (_applicationController != null)
            SearchBoxQuery = string.Empty;
    }

    [RelayCommand]
    private void ShowSettings()
    {
        if (_applicationController == null)
            return;
        if (_settingsWindow == null)
        {
            _settingsWindow = new SettingsWindow();
            _settingsWindow.DataContext =
                new SettingsWindowViewModel(_settingsWindow, _applicationController);
            _settingsWindow.Closed += (_, _) => { _settingsWindow = null; };
        }

        _mainWindow.Hide();
        _settingsWindow.Show();
        _settingsWindow.BringIntoView();
        _settingsWindow.Focus();
    }

    [RelayCommand]
    private void Sync()
    {
        if (_applicationController == null)
            return;
        if (IsSyncing)
            return;
        Task.Run(async () =>
        {
            Dispatcher.UIThread.Invoke(() => IsSyncing = true);
            try
            {
                await _applicationController.Sync();
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    IsSyncing = false;
                    var box = MessageBoxManager.GetMessageBoxStandard("Error",
                                                                      $"{ex.Message}\r\n\r\n${ex.StackTrace}",
                                                                      ButtonEnum.Ok,
                                                                      Icon.Error);
                    await box.ShowWindowAsync();
                });
            }

            Dispatcher.UIThread.Invoke(() => SearchBoxQuery = SearchBoxQuery);
            Dispatcher.UIThread.Invoke(() => IsSyncing = false);
        });
    }
}
