using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClipboardPilot.Domain.Entities;
using ClipboardPilot.Domain.Interfaces;
using ClipboardPilot.Services;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClipboardPilot.ViewModels;

public partial class MiniPanelViewModel : ObservableObject
{
    private readonly IClipboardRepository _repository;
    private readonly ImageService _imageService;
    private readonly ClipboardWatcher _clipboardWatcher;
    private readonly ILogger _logger;

    [ObservableProperty]
    private ObservableCollection<ClipboardItem> _recentItems = new();

    [ObservableProperty]
    private ClipboardItem? _selectedItem;

    [ObservableProperty]
    private string _quickSearchText = string.Empty;

    [ObservableProperty]
    private int _selectedIndex;

    public MiniPanelViewModel(
        IClipboardRepository repository,
        ImageService imageService,
        ClipboardWatcher clipboardWatcher,
        ILogger logger)
    {
        _repository = repository;
        _imageService = imageService;
        _clipboardWatcher = clipboardWatcher;
        _logger = logger;
    }

    public async Task LoadRecentItemsAsync()
    {
        try
        {
            var items = await _repository.GetRecentAsync(20);
            RecentItems.Clear();
            foreach (var item in items)
            {
                RecentItems.Add(item);
            }

            if (RecentItems.Any())
            {
                SelectedIndex = 0;
                SelectedItem = RecentItems[0];
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load recent items");
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(QuickSearchText))
            {
                await LoadRecentItemsAsync();
            }
            else
            {
                var results = (await _repository.SearchAsync(QuickSearchText)).Take(20);
                RecentItems.Clear();
                foreach (var item in results)
                {
                    RecentItems.Add(item);
                }

                if (RecentItems.Any())
                {
                    SelectedIndex = 0;
                    SelectedItem = RecentItems[0];
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Quick search failed");
        }
    }

    [RelayCommand]
    private async Task PasteSelectedAsync()
    {
        if (SelectedItem == null) return;

        try
        {
            await CopyItemToClipboardAsync(SelectedItem);
            _logger.Information("Item pasted from mini panel: {Type}", SelectedItem.Type);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to paste from mini panel");
        }
    }

    [RelayCommand]
    private async Task PasteFavoriteAsync(int rank)
    {
        try
        {
            var items = await _repository.GetByFavoriteRankAsync(rank);
            var item = items.FirstOrDefault();
            
            if (item != null)
            {
                await CopyItemToClipboardAsync(item);
                _logger.Information("Favorite item pasted: Rank {Rank}", rank);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to paste favorite item: {Rank}", rank);
        }
    }

    [RelayCommand]
    private void MoveSelectionUp()
    {
        if (SelectedIndex > 0)
        {
            SelectedIndex--;
            SelectedItem = RecentItems[SelectedIndex];
        }
    }

    [RelayCommand]
    private void MoveSelectionDown()
    {
        if (SelectedIndex < RecentItems.Count - 1)
        {
            SelectedIndex++;
            SelectedItem = RecentItems[SelectedIndex];
        }
    }

    private async Task CopyItemToClipboardAsync(ClipboardItem item)
    {
        _clipboardWatcher.SetClipboardData(() =>
        {
            switch (item.Type)
            {
                case Domain.Enums.ClipboardType.Text:
                    if (!string.IsNullOrEmpty(item.Text))
                        Clipboard.SetText(item.Text);
                    break;
                case Domain.Enums.ClipboardType.Html:
                    if (!string.IsNullOrEmpty(item.Html))
                        Clipboard.SetText(item.Html);
                    break;
                case Domain.Enums.ClipboardType.Rtf:
                    if (!string.IsNullOrEmpty(item.Rtf))
                        Clipboard.SetText(item.Rtf);
                    break;
                case Domain.Enums.ClipboardType.Image:
                    byte[]? imageBytes = null;
                    if (item.ImageBytes != null)
                    {
                        imageBytes = item.ImageBytes;
                    }
                    else if (!string.IsNullOrEmpty(item.ImagePath))
                    {
                        // Handle async image loading
                        Task.Run(async () =>
                        {
                            var bytes = await _imageService.LoadFromCache(item.ImagePath);
                            if (bytes != null)
                            {
                                await Application.Current.Dispatcher.InvokeAsync(() =>
                                {
                                    _clipboardWatcher.SetClipboardData(() =>
                                    {
                                        var bitmap = _imageService.BytesToBitmapSource(bytes);
                                        if (bitmap != null)
                                            Clipboard.SetImage(bitmap);
                                    });
                                });
                            }
                        });
                        return; // Exit early for async case
                    }

                    if (imageBytes != null)
                    {
                        var bitmap = _imageService.BytesToBitmapSource(imageBytes);
                        if (bitmap != null)
                            Clipboard.SetImage(bitmap);
                    }
                    break;
                case Domain.Enums.ClipboardType.FileList:
                    if (!string.IsNullOrEmpty(item.FileList))
                    {
                        var files = item.FileList.Split('\n');
                        var collection = new System.Collections.Specialized.StringCollection();
                        collection.AddRange(files);
                        Clipboard.SetFileDropList(collection);
                    }
                    break;
            }
        });
    }
}
