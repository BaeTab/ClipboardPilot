using System;

namespace ClipboardPilot.Models;

public class AppSettings
{
    public GeneralSettings General { get; set; } = new();
    public CollectionSettings Collection { get; set; } = new();
    public PasteSettings Paste { get; set; } = new();
    public HotkeySettings Hotkeys { get; set; } = new();
    public ThemeSettings Theme { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
}

public class GeneralSettings
{
    public bool StartWithWindows { get; set; } = true;
    public int StartupDelay { get; set; } = 3;
    public int MaxItemsToKeep { get; set; } = 1000;
}

public class CollectionSettings
{
    public bool Enabled { get; set; } = true;
    public bool FilterSensitiveData { get; set; } = true;
    public bool SaveImages { get; set; } = true;
    public string ImageStorageMode { get; set; } = "Database"; // Database or FileCache
    public int ThumbnailSize { get; set; } = 256;
    public int MaxImageSizeMB { get; set; } = 15;
}

public class PasteSettings
{
    public string DefaultFormat { get; set; } = "Text";
    public string MultiItemSeparator { get; set; } = "\n";
    public string LineEndingStyle { get; set; } = "Windows"; // Windows or Mac
}

public class HotkeySettings
{
    public string ShowMiniPanel { get; set; } = "Ctrl+Shift+V";
    public string PastePrevious { get; set; } = "Ctrl+Alt+V";
    public string QuickLock { get; set; } = "Ctrl+Alt+L";
}

public class ThemeSettings
{
    public string CurrentTheme { get; set; } = "TheBezierLight";
    public bool UseDarkMode { get; set; } = false;
}

public class SecuritySettings
{
    public bool RequirePin { get; set; } = false;
    public string? PinHash { get; set; }
}
