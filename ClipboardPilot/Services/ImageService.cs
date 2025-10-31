using Serilog;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace ClipboardPilot.Services;

public class ImageService
{
    private readonly ILogger _logger;
    private readonly string _cacheDirectory;

    public ImageService(ILogger logger)
    {
        _logger = logger;
        _cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClipboardPilot",
            "Images");

        if (!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
    }

    public byte[]? BitmapSourceToBytes(BitmapSource bitmapSource)
    {
        try
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using var stream = new MemoryStream();
            encoder.Save(stream);
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to convert BitmapSource to bytes");
            return null;
        }
    }

    public BitmapSource? BytesToBitmapSource(byte[] bytes)
    {
        try
        {
            using var stream = new MemoryStream(bytes);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to convert bytes to BitmapSource");
            return null;
        }
    }

    public byte[]? CreateThumbnail(byte[] imageBytes, int maxSize = 256)
    {
        try
        {
            var source = BytesToBitmapSource(imageBytes);
            if (source == null) return null;

            double scale = Math.Min(maxSize / (double)source.PixelWidth, maxSize / (double)source.PixelHeight);
            
            if (scale >= 1) return imageBytes; // Already small enough

            int thumbnailWidth = (int)(source.PixelWidth * scale);
            int thumbnailHeight = (int)(source.PixelHeight * scale);

            var thumbnail = new TransformedBitmap(source, new ScaleTransform(scale, scale));
            
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(thumbnail));

            using var stream = new MemoryStream();
            encoder.Save(stream);
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to create thumbnail");
            return imageBytes;
        }
    }

    public async Task<string> SaveToCache(byte[] imageBytes, Guid id)
    {
        try
        {
            var filename = $"{id}.png";
            var path = Path.Combine(_cacheDirectory, filename);
            await File.WriteAllBytesAsync(path, imageBytes);
            _logger.Information("Image saved to cache: {Path}", path);
            return path;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save image to cache");
            throw;
        }
    }

    public async Task<byte[]?> LoadFromCache(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                return await File.ReadAllBytesAsync(path);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load image from cache: {Path}", path);
        }
        return null;
    }

    public void DeleteFromCache(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                _logger.Information("Image deleted from cache: {Path}", path);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to delete image from cache: {Path}", path);
        }
    }
}
