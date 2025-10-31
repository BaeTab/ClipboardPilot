using DevExpress.Xpf.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ClipboardPilot.Infrastructure.Data;
using ClipboardPilot.Infrastructure.Repositories;
using ClipboardPilot.Domain.Interfaces;
using ClipboardPilot.Services;
using ClipboardPilot.ViewModels;
using Serilog;
using System.IO;
using System.Windows;
using System;

namespace ClipboardPilot;

public partial class App : Application
{
    private IHost? _host;

    static App()
    {
        CompatibilitySettings.UseLightweightThemes = true;
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Setup Serilog
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClipboardPilot",
            "Logs",
            "clipboard-pilot-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("=== Clipboard Pilot Starting ===");

        try
        {
            // Build Host
            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    // Database
                    var dbPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "ClipboardPilot",
                        "clipboard-pilot.db");

                    var dbDirectory = Path.GetDirectoryName(dbPath);
                    if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
                    {
                        Directory.CreateDirectory(dbDirectory);
                    }

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlite($"Data Source={dbPath}"));

                    // Repository & UnitOfWork
                    services.AddScoped<IClipboardRepository, ClipboardRepository>();
                    services.AddScoped<IUnitOfWork, UnitOfWork>();

                    // Services
                    services.AddSingleton<ILogger>(Log.Logger);
                    services.AddSingleton<SettingsService>();
                    services.AddSingleton<ImageService>();
                    services.AddSingleton<HotkeyService>();
                    services.AddSingleton<ClipboardWatcher>();

                    // ViewModels
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<SettingsViewModel>();
                    services.AddTransient<MiniPanelViewModel>();

                    // Windows
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            // Initialize Database
            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Database.EnsureCreatedAsync();
                Log.Information("Database initialized");
            }

            await _host.StartAsync();

            // Show Main Window
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            Log.Information("Application started successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start");
            MessageBox.Show($"Application failed to start:\n{ex.Message}", 
                "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            Log.Information("=== Clipboard Pilot Stopped ===");
            Log.CloseAndFlush();
        }
        finally
        {
            base.OnExit(e);
        }
    }
}
