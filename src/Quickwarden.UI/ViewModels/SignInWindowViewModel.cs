using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Quickwarden.Application;
using Quickwarden.Application.PlugIns.FrontEnd;
using Quickwarden.UI.Views;

namespace Quickwarden.UI.ViewModels;

public partial class SignInWindowViewModel : ViewModelBase
{
    private readonly ApplicationController _applicationController;
    private readonly SignInWindow _signInWindow;
    private readonly SettingsWindowViewModel _settingsWindow;
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _needs2Fa;
    private string _totp = string.Empty;
    public bool Needs2Fa
    {
        get => _needs2Fa;
        set
        {
            _needs2Fa = value;
            OnPropertyChanged();
        }
    }
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }
    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged();
        }
    }
    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
        }
    }
    
    public string Totp
    {
        get => _totp;
        set
        {
            _totp = value;
            OnPropertyChanged();
        }
    }

    public SignInWindowViewModel(ApplicationController applicationController,
                                 SignInWindow signInWindow,
                                 SettingsWindowViewModel settingsWindow)
    {
        _applicationController = applicationController;
        _signInWindow = signInWindow;
        _settingsWindow = settingsWindow;
    }

    [RelayCommand]
    public void SignIn()
    {
        IsLoading = true;
        Task.Run(async () =>
        {
            try
            {
                var result = await _applicationController.SignIn(Username,
                                                                 Password,
                                                                 Totp,
                                                                 CancellationToken.None);
                if (result == SignInResult.Success)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        _signInWindow.Close();
                        _settingsWindow.RefreshAccounts();
                    });
                    return;
                }

                if (result == SignInResult.Missing2Fa)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        Needs2Fa = true;
                        ErrorMessage = string.Empty;
                        IsLoading = false;
                    });
                    return;
                }

                string errorMessage;
                if (result == SignInResult.AlreadySignedIn)
                {
                    errorMessage = "You are already signed in.";
                }
                else if (result == SignInResult.Missing2Fa)
                {
                    errorMessage = "You are missing a 2FA.";
                }
                else if (result == SignInResult.Timeout)
                {
                    errorMessage = "Time-out.";
                }
                else if (result == SignInResult.WrongCredentials)
                {
                    errorMessage = "Wrong credentials.";
                }
                else
                {
                    errorMessage = "Unknown error.";
                }

                Dispatcher
                    .UIThread
                    .Invoke(() =>
                    {
                        ErrorMessage = errorMessage;
                        IsLoading = false;
                    });
            }
            catch (Exception e)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    IsLoading = false;
                    var box = MessageBoxManager.GetMessageBoxStandard("Error",
                                                                      $"{e.Message}\r\n\r\n${e.StackTrace}",
                                                                      ButtonEnum.Ok,
                                                                      Icon.Error);
                    await box.ShowWindowDialogAsync(_signInWindow);
                });
            }
        });
    }
}
