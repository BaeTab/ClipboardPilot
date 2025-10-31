using ClipboardPilot.Domain.Entities;
using ClipboardPilot.Domain.Enums;
using ClipboardPilot.Domain.Interfaces;
using Serilog;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace ClipboardPilot.Services;

public class ClipboardWatcher : IDisposable
{
    private readonly ILogger _logger;
    private readonly IClipboardRepository _repository;
    private readonly ImageService _imageService;
    private readonly SettingsService _settingsService;
    private HwndSource? _hwndSource;
    private IntPtr _nextClipboardViewer;
    private readonly Dictionary<string, DateTime> _recentHashes = new();
    private const int MAX_RETRIES = 5;
    private readonly int[] _retryDelays = { 100, 300, 500, 800, 1000 };
    
    // 자기 자신의 프로세스 이름
    private readonly string _currentProcessName;
    
    // 프로그램이 클립보드에 데이터를 설정하는 중인지 추적
    private bool _isSettingClipboard = false;

    private const int WM_DRAWCLIPBOARD = 0x0308;
    private const int WM_CHANGECBCHAIN = 0x030D;

    [DllImport("user32.dll")]
    private static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

    [DllImport("user32.dll")]
    private static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    public event EventHandler<ClipboardItem>? ItemAdded;

    public ClipboardWatcher(
        ILogger logger,
        IClipboardRepository repository,
        ImageService imageService,
        SettingsService settingsService)
    {
        _logger = logger;
        _repository = repository;
        _imageService = imageService;
        _settingsService = settingsService;
        
        // 현재 프로세스 이름 저장
        _currentProcessName = Process.GetCurrentProcess().ProcessName;
    }

    public void Start(Window window)
    {
        try
        {
            var helper = new WindowInteropHelper(window);
            _hwndSource = HwndSource.FromHwnd(helper.Handle);
            _hwndSource?.AddHook(WndProc);
            _nextClipboardViewer = SetClipboardViewer(helper.Handle);
            _logger.Information("Clipboard watcher started");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to start clipboard watcher");
        }
    }

    public void Stop()
    {
        try
        {
            if (_hwndSource != null)
            {
                ChangeClipboardChain(_hwndSource.Handle, _nextClipboardViewer);
                _hwndSource.RemoveHook(WndProc);
                _logger.Information("Clipboard watcher stopped");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to stop clipboard watcher");
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_DRAWCLIPBOARD:
                _ = ProcessClipboardChangeAsync();
                SendMessage(_nextClipboardViewer, msg, wParam, lParam);
                break;

            case WM_CHANGECBCHAIN:
                if (wParam == _nextClipboardViewer)
                    _nextClipboardViewer = lParam;
                else
                    SendMessage(_nextClipboardViewer, msg, wParam, lParam);
                break;
        }

        return IntPtr.Zero;
    }

    private async Task ProcessClipboardChangeAsync()
    {
        if (!_settingsService.Settings.Collection.Enabled)
            return;

        for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await CaptureClipboardDataAsync();
                });
                return;
            }
            catch (COMException ex) when (ex.HResult == unchecked((int)0x800401D0)) // CLIPBRD_E_CANT_OPEN
            {
                if (attempt < MAX_RETRIES - 1)
                {
                    await Task.Delay(_retryDelays[attempt]);
                }
                else
                {
                    _logger.Warning("Clipboard access failed after {Retries} retries", MAX_RETRIES);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing clipboard change");
                return;
            }
        }
    }

    private async Task CaptureClipboardDataAsync()
    {
        // 프로그램이 직접 클립보드에 설정한 경우 무시
        if (_isSettingClipboard)
        {
            _logger.Debug("Ignoring clipboard change initiated by self");
            return;
        }
        
        var dataObject = Clipboard.GetDataObject();
        if (dataObject == null) return;

        ClipboardItem? item = null;
        var sourceApp = GetForegroundWindowInfo();
        
        // 자기 자신의 프로세스에서 복사한 경우 무시
        if (IsOwnProcess(sourceApp))
        {
            _logger.Debug("Ignoring clipboard change from own process: {SourceApp}", sourceApp);
            return;
        }

        // Text
        if (dataObject.GetDataPresent(DataFormats.UnicodeText))
        {
            var text = dataObject.GetData(DataFormats.UnicodeText) as string;
            if (!string.IsNullOrEmpty(text))
            {
                item = await CreateTextItemAsync(text, sourceApp);
            }
        }
        // HTML
        else if (dataObject.GetDataPresent(DataFormats.Html))
        {
            var html = dataObject.GetData(DataFormats.Html) as string;
            if (!string.IsNullOrEmpty(html))
            {
                item = await CreateHtmlItemAsync(html, sourceApp);
            }
        }
        // RTF
        else if (dataObject.GetDataPresent(DataFormats.Rtf))
        {
            var rtf = dataObject.GetData(DataFormats.Rtf) as string;
            if (!string.IsNullOrEmpty(rtf))
            {
                item = await CreateRtfItemAsync(rtf, sourceApp);
            }
        }
        // Image
        else if (dataObject.GetDataPresent(DataFormats.Bitmap))
        {
            var bitmap = dataObject.GetData(DataFormats.Bitmap) as BitmapSource;
            if (bitmap != null && _settingsService.Settings.Collection.SaveImages)
            {
                item = await CreateImageItemAsync(bitmap, sourceApp);
            }
        }
        // File List
        else if (dataObject.GetDataPresent(DataFormats.FileDrop))
        {
            var files = dataObject.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length > 0)
            {
                item = await CreateFileListItemAsync(files, sourceApp);
            }
        }

        if (item != null)
        {
            await SaveItemAsync(item);
        }
    }

    private async Task<ClipboardItem?> CreateTextItemAsync(string text, string sourceApp)
    {
        if (text.Length < 3 || string.IsNullOrWhiteSpace(text))
            return null;

        if (_settingsService.Settings.Collection.FilterSensitiveData && IsSensitiveText(text))
        {
            _logger.Information("Sensitive text filtered, length: {Length}", text.Length);
            return null;
        }

        var hash = ComputeHash(text);
        if (IsDuplicate(hash))
            return null;

        var preview = text.Length > 200 ? text.Substring(0, 200) + "..." : text;

        return new ClipboardItem
        {
            Type = ClipboardType.Text,
            Text = text,
            Preview = preview,
            SourceApp = sourceApp,
            Size = Encoding.UTF8.GetByteCount(text),
            Hash = hash
        };
    }

    private async Task<ClipboardItem?> CreateHtmlItemAsync(string html, string sourceApp)
    {
        var plainText = StripHtml(html);
        if (string.IsNullOrWhiteSpace(plainText) || plainText.Length < 3)
            return null;

        var hash = ComputeHash(html);
        if (IsDuplicate(hash))
            return null;

        var preview = plainText.Length > 200 ? plainText.Substring(0, 200) + "..." : plainText;

        return new ClipboardItem
        {
            Type = ClipboardType.Html,
            Html = html,
            Text = plainText,
            Preview = preview,
            SourceApp = sourceApp,
            Size = Encoding.UTF8.GetByteCount(html),
            Hash = hash
        };
    }

    private async Task<ClipboardItem?> CreateRtfItemAsync(string rtf, string sourceApp)
    {
        var hash = ComputeHash(rtf);
        if (IsDuplicate(hash))
            return null;

        var preview = rtf.Length > 200 ? rtf.Substring(0, 200) + "..." : rtf;

        return new ClipboardItem
        {
            Type = ClipboardType.Rtf,
            Rtf = rtf,
            Preview = preview,
            SourceApp = sourceApp,
            Size = Encoding.UTF8.GetByteCount(rtf),
            Hash = hash
        };
    }

    private async Task<ClipboardItem?> CreateImageItemAsync(BitmapSource bitmap, string sourceApp)
    {
        var imageBytes = _imageService.BitmapSourceToBytes(bitmap);
        if (imageBytes == null)
            return null;

        var sizeMB = imageBytes.Length / (1024.0 * 1024.0);
        if (sizeMB > _settingsService.Settings.Collection.MaxImageSizeMB)
        {
            _logger.Warning("Image too large: {Size}MB", sizeMB);
            return null;
        }

        var hash = ComputeHash(imageBytes);
        if (IsDuplicate(hash))
            return null;

        var thumbnail = _imageService.CreateThumbnail(imageBytes, _settingsService.Settings.Collection.ThumbnailSize);

        var item = new ClipboardItem
        {
            Type = ClipboardType.Image,
            Preview = $"Image {bitmap.PixelWidth}x{bitmap.PixelHeight}",
            SourceApp = sourceApp,
            Size = imageBytes.Length,
            Hash = hash
        };

        if (_settingsService.Settings.Collection.ImageStorageMode == "Database")
        {
            item.ImageBytes = thumbnail ?? imageBytes;
        }
        else
        {
            var path = await _imageService.SaveToCache(thumbnail ?? imageBytes, item.Id);
            item.ImagePath = path;
        }

        return item;
    }

    private async Task<ClipboardItem?> CreateFileListItemAsync(string[] files, string sourceApp)
    {
        var fileList = string.Join("\n", files);
        var hash = ComputeHash(fileList);
        
        if (IsDuplicate(hash))
            return null;

        var preview = files.Length == 1 ? Path.GetFileName(files[0]) : $"{files.Length} files";

        return new ClipboardItem
        {
            Type = ClipboardType.FileList,
            FileList = fileList,
            Preview = preview,
            SourceApp = sourceApp,
            Size = Encoding.UTF8.GetByteCount(fileList),
            Hash = hash
        };
    }

    private async Task SaveItemAsync(ClipboardItem item)
    {
        try
        {
            await _repository.AddAsync(item);
            
            // Enforce max items limit
            var count = await _repository.GetCountAsync();
            if (count > _settingsService.Settings.General.MaxItemsToKeep)
            {
                var oldestDate = DateTime.Now.AddDays(-30);
                await _repository.DeleteOlderThanAsync(oldestDate);
            }

            _logger.Information("Clipboard item saved: Type={Type}, Size={Size}", item.Type, item.Size);
            ItemAdded?.Invoke(this, item);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save clipboard item");
        }
    }

    private bool IsSensitiveText(string text)
    {
        // Password pattern: 8-72 chars with mixed case, numbers, special chars
        var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,72}$";
        if (Regex.IsMatch(text, passwordPattern))
            return true;

        // Common password keywords
        var sensitiveKeywords = new[] { "password", "passwd", "pwd", "secret", "token", "apikey", "api_key" };
        return sensitiveKeywords.Any(keyword => text.ToLower().Contains(keyword));
    }

    private string StripHtml(string html)
    {
        return Regex.Replace(html, "<.*?>", string.Empty);
    }

    private string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private string ComputeHash(byte[] input)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(input);
        return Convert.ToBase64String(hash);
    }

    private bool IsDuplicate(string hash)
    {
        var cutoff = DateTime.Now.AddSeconds(-10);
        
        // Clean old hashes
        var oldKeys = _recentHashes.Where(kvp => kvp.Value < cutoff).Select(kvp => kvp.Key).ToList();
        foreach (var key in oldKeys)
            _recentHashes.Remove(key);

        if (_recentHashes.ContainsKey(hash))
            return true;

        _recentHashes[hash] = DateTime.Now;
        return false;
    }

    private string GetForegroundWindowInfo()
    {
        try
        {
            var hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
                return "Unknown";

            GetWindowThreadProcessId(hwnd, out uint processId);
            var process = Process.GetProcessById((int)processId);
            return process.ProcessName;
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 주어진 소스 앱이 현재 프로그램인지 확인
    /// </summary>
    private bool IsOwnProcess(string sourceApp)
    {
        if (string.IsNullOrEmpty(sourceApp))
            return false;
            
        // ClipboardPilot 프로세스 이름과 비교
        return sourceApp.Equals(_currentProcessName, StringComparison.OrdinalIgnoreCase) ||
               sourceApp.Contains("ClipboardPilot", StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// 프로그램에서 클립보드에 데이터를 설정할 때 호출
    /// </summary>
    public void SetClipboardData(Action setAction)
    {
        try
        {
            _isSettingClipboard = true;
            setAction();
        }
        finally
        {
            // 짧은 지연 후 플래그 해제
            Task.Delay(500).ContinueWith(_ => _isSettingClipboard = false);
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
