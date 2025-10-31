using Serilog;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace ClipboardPilot.Services;

public class HotkeyService : IDisposable
{
    private readonly ILogger _logger;
    private readonly Dictionary<int, Action> _hotkeyCallbacks = new();
    private int _currentHotkeyId = 9000;
    private IntPtr _windowHandle;

    private const int WM_HOTKEY = 0x0312;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    // Modifiers
    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;

    public HotkeyService(ILogger logger)
    {
        _logger = logger;
    }

    public void Initialize(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
        ComponentDispatcher.ThreadPreprocessMessage += OnThreadPreprocessMessage;
    }

    public bool RegisterHotkey(string hotkeyString, Action callback)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hotkeyString))
                return false;

            var (modifiers, key) = ParseHotkeyString(hotkeyString);
            if (key == 0)
            {
                _logger.Warning("Invalid hotkey string: {Hotkey}", hotkeyString);
                return false;
            }

            var hotkeyId = _currentHotkeyId++;
            
            if (RegisterHotKey(_windowHandle, hotkeyId, modifiers, key))
            {
                _hotkeyCallbacks[hotkeyId] = callback;
                _logger.Information("Hotkey registered: {Hotkey} with ID {Id}", hotkeyString, hotkeyId);
                return true;
            }
            else
            {
                _logger.Warning("Failed to register hotkey: {Hotkey} (may be in use by another application)", hotkeyString);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error registering hotkey: {Hotkey}", hotkeyString);
            return false;
        }
    }

    public void UnregisterHotkey(int hotkeyId)
    {
        try
        {
            if (_hotkeyCallbacks.ContainsKey(hotkeyId))
            {
                UnregisterHotKey(_windowHandle, hotkeyId);
                _hotkeyCallbacks.Remove(hotkeyId);
                _logger.Information("Hotkey unregistered: ID {Id}", hotkeyId);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error unregistering hotkey: {Id}", hotkeyId);
        }
    }

    public void UnregisterAll()
    {
        foreach (var hotkeyId in _hotkeyCallbacks.Keys.ToList())
        {
            UnregisterHotkey(hotkeyId);
        }
    }

    private void OnThreadPreprocessMessage(ref MSG msg, ref bool handled)
    {
        if (msg.message == WM_HOTKEY)
        {
            var hotkeyId = msg.wParam.ToInt32();
            if (_hotkeyCallbacks.TryGetValue(hotkeyId, out var callback))
            {
                try
                {
                    callback?.Invoke();
                    handled = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error executing hotkey callback for ID {Id}", hotkeyId);
                }
            }
        }
    }

    private (uint modifiers, uint key) ParseHotkeyString(string hotkeyString)
    {
        uint modifiers = 0;
        uint key = 0;

        var parts = hotkeyString.Split('+');
        foreach (var part in parts)
        {
            var normalized = part.Trim().ToLower();
            switch (normalized)
            {
                case "ctrl":
                case "control":
                    modifiers |= MOD_CONTROL;
                    break;
                case "alt":
                    modifiers |= MOD_ALT;
                    break;
                case "shift":
                    modifiers |= MOD_SHIFT;
                    break;
                case "win":
                case "windows":
                    modifiers |= MOD_WIN;
                    break;
                default:
                    // Try to parse as key
                    if (Enum.TryParse<Key>(part.Trim(), true, out var parsedKey))
                    {
                        key = (uint)KeyInterop.VirtualKeyFromKey(parsedKey);
                    }
                    break;
            }
        }

        return (modifiers, key);
    }

    public bool TestHotkey(string hotkeyString)
    {
        var (modifiers, key) = ParseHotkeyString(hotkeyString);
        if (key == 0) return false;

        var testId = 8999;
        var result = RegisterHotKey(_windowHandle, testId, modifiers, key);
        
        if (result)
        {
            UnregisterHotKey(_windowHandle, testId);
        }

        return result;
    }

    public void Dispose()
    {
        UnregisterAll();
        ComponentDispatcher.ThreadPreprocessMessage -= OnThreadPreprocessMessage;
    }
}
