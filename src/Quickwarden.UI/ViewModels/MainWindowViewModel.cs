using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
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
    public string SettingsShortcut => OperatingSystem.IsMacOS() ? "⌘S" : "Ctrl-S";
    public KeyGesture CopyUsernameGesture => OperatingSystem.IsMacOS()
                                                 ? new KeyGesture(Key.U, KeyModifiers.Meta)
                                                 : new KeyGesture(Key.U, KeyModifiers.Control);
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
    public void CopyUsername()
    {
        if (SelectedCredential == null || _applicationController == null)
            return;
        _mainWindow.Clipboard.SetTextAsync(_applicationController.GetUsername(SelectedCredential.Id));
        Hide();
    }

    [RelayCommand]
    public void CopyPassword()
    {
        if (SelectedCredential == null || _applicationController == null)
            return;
        _mainWindow.Clipboard.SetTextAsync(_applicationController.GetPassword(SelectedCredential.Id));
        Hide();
    }

    [RelayCommand]
    public void Copy2Fa()
    {
        if (SelectedCredential == null || _applicationController == null)
            return;
        _mainWindow.Clipboard.SetTextAsync(_applicationController.GetTotp(SelectedCredential.Id).Code);
        Hide();
    }

    public void SetApplicationController(ApplicationController applicationController)
    {
        _applicationController = applicationController;
        OnPropertyChanged(nameof(ApplicationInitialized));
        OnPropertyChanged(nameof(SearchBoxWatermark));
        _mainWindow.SearchBox.Focus();
    }

    [RelayCommand]
    public void Hide()
    {
        _mainWindow.Hide();
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
}
