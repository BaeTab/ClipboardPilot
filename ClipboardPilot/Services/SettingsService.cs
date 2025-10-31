using Microsoft.Extensions.Options;
using Serilog;
using System.IO;
using System.Text.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using ClipboardPilot.Models;

namespace ClipboardPilot.Services;

public class SettingsService
{
    private readonly string _settingsPath;
    private AppSettings _settings;
    private readonly ILogger _logger;

    public SettingsService(ILogger logger)
    {
        _logger = logger;
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClipboardPilot",
            "clipboard-pilot.settings.json");
        
        _settings = LoadSettings();
    }

    public AppSettings Settings => _settings;

    private AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                {
                    _logger.Information("Settings loaded successfully from {Path}", _settingsPath);
                    return settings;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load settings from {Path}", _settingsPath);
        }

        _logger.Information("Using default settings");
        return new AppSettings();
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(_settings, options);
            await File.WriteAllTextAsync(_settingsPath, json);
            
            _logger.Information("Settings saved successfully to {Path}", _settingsPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save settings to {Path}", _settingsPath);
            throw;
        }
    }

    public void UpdateSettings(AppSettings newSettings)
    {
        _settings = newSettings;
    }

    public string GetSettingsPath() => _settingsPath;
}
