using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClipboardPilot.Services;
using Serilog;
using System.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClipboardPilot.Models;

namespace ClipboardPilot.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly ILogger _logger;

    [ObservableProperty]
    private AppSettings _settings;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SettingsViewModel(SettingsService settingsService, ILogger logger)
    {
        _settingsService = settingsService;
        _logger = logger;
        _settings = new AppSettings
        {
            General = new GeneralSettings
            {
                StartWithWindows = _settingsService.Settings.General.StartWithWindows,
                StartupDelay = _settingsService.Settings.General.StartupDelay,
                MaxItemsToKeep = _settingsService.Settings.General.MaxItemsToKeep
            },
            Collection = new CollectionSettings
            {
                Enabled = _settingsService.Settings.Collection.Enabled,
                FilterSensitiveData = _settingsService.Settings.Collection.FilterSensitiveData,
                SaveImages = _settingsService.Settings.Collection.SaveImages,
                ImageStorageMode = _settingsService.Settings.Collection.ImageStorageMode,
                ThumbnailSize = _settingsService.Settings.Collection.ThumbnailSize,
                MaxImageSizeMB = _settingsService.Settings.Collection.MaxImageSizeMB
            },
            Paste = new PasteSettings
            {
                DefaultFormat = _settingsService.Settings.Paste.DefaultFormat,
                MultiItemSeparator = _settingsService.Settings.Paste.MultiItemSeparator,
                LineEndingStyle = _settingsService.Settings.Paste.LineEndingStyle
            },
            Hotkeys = new HotkeySettings
            {
                ShowMiniPanel = _settingsService.Settings.Hotkeys.ShowMiniPanel,
                PastePrevious = _settingsService.Settings.Hotkeys.PastePrevious,
                QuickLock = _settingsService.Settings.Hotkeys.QuickLock
            },
            Theme = new ThemeSettings
            {
                CurrentTheme = _settingsService.Settings.Theme.CurrentTheme,
                UseDarkMode = _settingsService.Settings.Theme.UseDarkMode
            },
            Security = new SecuritySettings
            {
                RequirePin = _settingsService.Settings.Security.RequirePin,
                PinHash = _settingsService.Settings.Security.PinHash
            }
        };
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            _settingsService.UpdateSettings(Settings);
            await _settingsService.SaveSettingsAsync();
            StatusMessage = "Settings saved successfully";
            _logger.Information("Settings saved");
            
            MessageBox.Show("Settings saved successfully. Some changes may require restart.", 
                "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save settings");
            StatusMessage = "Failed to save settings";
            MessageBox.Show("Failed to save settings: " + ex.Message, 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Reset()
    {
        var result = MessageBox.Show(
            "Are you sure you want to reset all settings to default?",
            "Confirm Reset",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            Settings = new AppSettings();
            StatusMessage = "Settings reset to default";
            _logger.Information("Settings reset to default");
        }
    }

    [RelayCommand]
    private void TestHotkey(string hotkeyName)
    {
        string? hotkey = hotkeyName switch
        {
            "MiniPanel" => Settings.Hotkeys.ShowMiniPanel,
            "PastePrevious" => Settings.Hotkeys.PastePrevious,
            "QuickLock" => Settings.Hotkeys.QuickLock,
            _ => null
        };

        if (!string.IsNullOrEmpty(hotkey))
        {
            MessageBox.Show($"Testing hotkey: {hotkey}\nPress the hotkey combination to test.",
                "Hotkey Test", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
