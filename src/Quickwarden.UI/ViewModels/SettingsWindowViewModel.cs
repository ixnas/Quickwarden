using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Quickwarden.Application;
using Quickwarden.UI.Views;

namespace Quickwarden.UI.ViewModels;

public class AccountModel
{
    public string Id { get; set; }
    public string Username { get; set; }

    public override string ToString()
    {
        return Username;
    }
}

public partial class SettingsWindowViewModel : ViewModelBase
{
    private readonly SettingsWindow _settingsWindow;
    private readonly ApplicationController _applicationController;
    private SignInWindow? _signInWindow;
    private AccountModel? _selectedAccount;
    private bool _isLoading;
    public AccountModel? SelectedAccount
    {
        get => _selectedAccount;
        set
        {
            _selectedAccount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RemoveButtonEnabled));
        }
    }
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RemoveButtonEnabled));
        }
    }
    public bool RemoveButtonEnabled => SelectedAccount != null && !IsLoading;

    public AccountModel[] Accounts => _applicationController.GetAccounts().Select(account =>
        new AccountModel
        {
            Id = account.Id,
            Username = account.Username,
        }).ToArray();

    public SettingsWindowViewModel(SettingsWindow settingsWindow, ApplicationController applicationController)
    {
        _settingsWindow = settingsWindow;
        _applicationController = applicationController;
    }

    [RelayCommand]
    private void Hide()
    {
        _settingsWindow.Close();
    }

    [RelayCommand]
    private async Task RemoveAccount()
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandard("Remove account",
            "Are you sure you want to remove this account?", ButtonEnum.YesNo, Icon.Question);
        var result = await messageBox.ShowWindowDialogAsync(_settingsWindow);
        if (result == ButtonResult.Yes && SelectedAccount != null)
        {
            Task.Run(async () =>
            {
                Dispatcher.UIThread.Invoke(() => IsLoading = true);
                await _applicationController.SignOut(SelectedAccount.Id);
                Dispatcher.UIThread.Invoke(RefreshAccounts);
                Dispatcher.UIThread.Invoke(() => IsLoading = false);
            });
        }
    }

    [RelayCommand]
    private void ShowSignInWindow()
    {
        if (_signInWindow == null)
        {
            _signInWindow = new SignInWindow();
            _signInWindow.DataContext = new SignInWindowViewModel(_applicationController, _signInWindow, this);
            _signInWindow.Closed += (_, _) => _signInWindow = null;
            _signInWindow.ShowDialog(_settingsWindow);
        }
    }

    public void RefreshAccounts()
    {
        OnPropertyChanged(nameof(Accounts));
    }
}
