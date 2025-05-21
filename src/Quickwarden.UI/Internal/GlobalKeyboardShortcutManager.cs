using System;
using Avalonia.Threading;
using Microsoft.Win32;
using SharpHook;
using SharpHook.Native;

namespace Quickwarden.UI.Internal;

internal class GlobalKeyboardShortcutManager : IDisposable
{
    private readonly Action _callback;
    private readonly SimpleGlobalHook _hook;

    private bool _ctrlDown;
    private bool _altDown;
    private bool _shiftDown;
    private bool _cmdDown;
    private bool _pDown;

    public GlobalKeyboardShortcutManager(Action callback)
    {
        if (OperatingSystem.IsWindows())
            SystemEvents.SessionSwitch += (_, _) => Reset();
        _callback = callback;
        _hook = new SimpleGlobalHook();
        _hook.KeyPressed += OnHookOnKeyPressed;
        _hook.KeyReleased += OnHookOnKeyReleased;
    }

    private void Reset()
    {
        _ctrlDown = _altDown = _shiftDown = _cmdDown = _pDown = false;
    }

    void OnHookOnKeyPressed(object? sender, KeyboardHookEventArgs args)
    {
        if (args.Data.KeyCode is KeyCode.VcLeftControl or KeyCode.VcRightControl)
        {
            _ctrlDown = true;
        }
        else if (args.Data.KeyCode is KeyCode.VcLeftAlt or KeyCode.VcRightAlt)
        {
            _altDown = true;
        }
        else if (args.Data.KeyCode is KeyCode.VcLeftShift or KeyCode.VcRightShift)
        {
            _shiftDown = true;
        }
        else if (args.Data.KeyCode is KeyCode.VcLeftMeta or KeyCode.VcRightMeta)
        {
            _cmdDown = true;
        }
        else if (args.Data.KeyCode == KeyCode.VcP)
        {
            _pDown = true;
        }

        if (CombinationPressed())
        {
            args.SuppressEvent = true;
            Dispatcher.UIThread.InvokeAsync(_callback);
        }
    }


    private void OnHookOnKeyReleased(object? sender, KeyboardHookEventArgs args)
    {
        if (args.Data.KeyCode is KeyCode.VcLeftControl or KeyCode.VcRightControl)
        {
            _ctrlDown = false;
        }
        else if (args.Data.KeyCode is KeyCode.VcLeftAlt or KeyCode.VcRightAlt)
        {
            _altDown = false;
        }
        else if (args.Data.KeyCode is KeyCode.VcLeftShift or KeyCode.VcRightShift)
        {
            _shiftDown = false;
        }
        else if (args.Data.KeyCode is KeyCode.VcLeftMeta or KeyCode.VcRightMeta)
        {
            _cmdDown = false;
        }
        else if (args.Data.KeyCode == KeyCode.VcP)
        {
            _pDown = false;
        }
    }

    public bool CombinationPressed()
    {
        if (OperatingSystem.IsMacOS())
        {
            return _altDown && !_shiftDown && !_ctrlDown && _cmdDown && _pDown;
        }

        return _altDown && !_shiftDown && _ctrlDown && !_cmdDown && _pDown;
    }

    public void StartListening()
    {
        _hook.RunAsync();
    }

    public void Dispose()
    {
        _hook.Dispose();
    }
}