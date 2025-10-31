using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClipboardPilot.Domain.Entities;
using ClipboardPilot.Domain.Interfaces;
using ClipboardPilot.Domain.Enums;
using ClipboardPilot.Services;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClipboardPilot.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IClipboardRepository _repository;
    private readonly ClipboardWatcher _clipboardWatcher;
    private readonly ImageService _imageService;
    private readonly SettingsService _settingsService;
    private readonly ILogger _logger;

    [ObservableProperty]
    private ObservableCollection<ClipboardItem> _items = new();

    [ObservableProperty]
    private ClipboardItem? _selectedItem;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ClipboardType? _filterType;

    [ObservableProperty]
    private ColorLabel? _filterLabel;

    [ObservableProperty]
    private bool _filterPinned;

    [ObservableProperty]
    private DateTime? _filterFromDate;

    [ObservableProperty]
    private DateTime? _filterToDate;

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _selectedCount;

    [ObservableProperty]
    private bool _isGridView = true;

    public MainViewModel(
        IClipboardRepository repository,
        ClipboardWatcher clipboardWatcher,
        ImageService imageService,
        SettingsService settingsService,
        ILogger logger)
    {
        _repository = repository;
        _clipboardWatcher = clipboardWatcher;
        _imageService = imageService;
        _settingsService = settingsService;
        _logger = logger;

        _clipboardWatcher.ItemAdded += OnClipboardItemAdded;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await LoadItemsAsync();
            StatusText = $"Loaded {Items.Count} items";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize MainViewModel");
            StatusText = "Error loading items";
        }
    }

    private async Task LoadItemsAsync()
    {
        try
        {
            var items = await _repository.GetAllAsync();
            
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Items.Clear();
                foreach (var item in items)
                {
                    Items.Add(item);
                }
                TotalCount = Items.Count;
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load items");
            throw;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            IEnumerable<ClipboardItem> results;
            
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                results = await _repository.GetAllAsync();
            }
            else
            {
                results = await _repository.SearchAsync(SearchText);
            }

            Application.Current?.Dispatcher.Invoke(() =>
            {
                Items.Clear();
                foreach (var item in results)
                {
                    Items.Add(item);
                }
                TotalCount = Items.Count;
            });
            
            StatusText = $"Found {Items.Count} items";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Search failed");
            StatusText = "Search error";
        }
    }

    [RelayCommand]
    private async Task ApplyFilterAsync()
    {
        try
        {
            var results = await _repository.FilterAsync(
                type: FilterType,
                fromDate: FilterFromDate,
                toDate: FilterToDate,
                label: FilterLabel,
                pinned: FilterPinned ? true : null);

            Application.Current?.Dispatcher.Invoke(() =>
            {
                Items.Clear();
                foreach (var item in results)
                {
                    Items.Add(item);
                }
                TotalCount = Items.Count;
            });
            
            StatusText = $"Filtered to {Items.Count} items";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Filter failed");
            StatusText = "Filter error";
        }
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        try
        {
            SearchText = string.Empty;
            FilterType = null;
            FilterLabel = null;
            FilterPinned = false;
            FilterFromDate = null;
            FilterToDate = null;
            await LoadItemsAsync();
            StatusText = "Filters cleared";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to clear filters");
            StatusText = "Error clearing filters";
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedItem == null)
        {
            StatusText = "No item selected";
            return;
        }

        try
        {
            var itemToDelete = SelectedItem;
            
            var result = MessageBox.Show(
                "Are you sure you want to delete this item?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Delete from database
                await _repository.DeleteAsync(itemToDelete.Id);
                
                // Remove from UI collection on UI thread
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    Items.Remove(itemToDelete);
                    TotalCount = Items.Count;
                    SelectedItem = null;
                });
                
                StatusText = "Item deleted";
                _logger.Information("Item deleted: {Id}", itemToDelete.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to delete item");
            StatusText = "Failed to delete item";
            
            Application.Current?.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"Failed to delete item: {ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            });
        }
    }

    [RelayCommand]
    private async Task TogglePinAsync()
    {
        if (SelectedItem == null)
        {
            StatusText = "No item selected";
            return;
        }

        try
        {
            var item = await _repository.GetByIdAsync(SelectedItem.Id);
            if (item != null)
            {
                item.Pinned = !item.Pinned;
                await _repository.UpdateAsync(item);
                
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    SelectedItem.Pinned = item.Pinned;
                });
                
                StatusText = item.Pinned ? "Item pinned" : "Item unpinned";
            }
            else
            {
                StatusText = "Item not found";
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to toggle pin");
            StatusText = "Failed to toggle pin";
            
            Application.Current?.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    "Failed to toggle pin", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            });
        }
    }

    [RelayCommand]
    private async Task SetFavoriteRankAsync(int rank)
    {
        if (SelectedItem == null)
        {
            StatusText = "No item selected";
            return;
        }
        
        if (rank < 1 || rank > 9)
        {
            StatusText = "Invalid rank (1-9)";
            return;
        }

        try
        {
            // Check if rank is already taken
            var existing = await _repository.GetByFavoriteRankAsync(rank);
            if (existing.Any())
            {
                var result = MessageBox.Show(
                    $"Favorite rank {rank} is already assigned. Replace it?",
                    "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;

                // Remove rank from existing item
                foreach (var item in existing)
                {
                    item.FavoriteRank = 0;
                    await _repository.UpdateAsync(item);
                }
            }

            var currentItem = await _repository.GetByIdAsync(SelectedItem.Id);
            if (currentItem != null)
            {
                currentItem.FavoriteRank = rank;
                await _repository.UpdateAsync(currentItem);
                
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    SelectedItem.FavoriteRank = rank;
                });
                
                StatusText = $"Favorite rank set to {rank}";
            }
            else
            {
                StatusText = "Item not found";
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to set favorite rank");
            StatusText = "Failed to set favorite rank";
            
            Application.Current?.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    "Failed to set favorite rank", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            });
        }
    }

    [RelayCommand]
    private async Task SetLabelAsync(ColorLabel label)
    {
        if (SelectedItem == null)
        {
            StatusText = "No item selected";
            return;
        }

        try
        {
            var item = await _repository.GetByIdAsync(SelectedItem.Id);
            if (item != null)
            {
                item.Label = label;
                await _repository.UpdateAsync(item);
                
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    SelectedItem.Label = label;
                });
                
                StatusText = $"Label set to {label}";
            }
            else
            {
                StatusText = "Item not found";
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to set label");
            StatusText = "Failed to set label";
            
            Application.Current?.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    "Failed to set label", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            });
        }
    }

    [RelayCommand]
    private async Task CopyToClipboardAsync()
    {
        if (SelectedItem == null)
        {
            StatusText = "No item selected";
            return;
        }

        try
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                // ClipboardWatcher에게 자체 설정임을 알림
                _clipboardWatcher.SetClipboardData(() =>
                {
                    switch (SelectedItem.Type)
                    {
                        case ClipboardType.Text:
                            if (!string.IsNullOrEmpty(SelectedItem.Text))
                                Clipboard.SetText(SelectedItem.Text);
                            break;
                            
                        case ClipboardType.Html:
                            if (!string.IsNullOrEmpty(SelectedItem.Html))
                                Clipboard.SetText(SelectedItem.Html);
                            break;
                            
                        case ClipboardType.Rtf:
                            if (!string.IsNullOrEmpty(SelectedItem.Rtf))
                                Clipboard.SetText(SelectedItem.Rtf);
                            break;
                            
                        case ClipboardType.Image:
                            byte[]? imageBytes = null;
                            if (SelectedItem.ImageBytes != null)
                            {
                                imageBytes = SelectedItem.ImageBytes;
                            }
                            else if (!string.IsNullOrEmpty(SelectedItem.ImagePath))
                            {
                                // Image loading is async, handle separately
                                Task.Run(async () =>
                                {
                                    var bytes = await _imageService.LoadFromCache(SelectedItem.ImagePath);
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
                            
                        case ClipboardType.FileList:
                            if (!string.IsNullOrEmpty(SelectedItem.FileList))
                            {
                                var files = SelectedItem.FileList.Split('\n');
                                var collection = new System.Collections.Specialized.StringCollection();
                                collection.AddRange(files);
                                Clipboard.SetFileDropList(collection);
                            }
                            break;
                    }
                });
            });
            
            StatusText = "Copied to clipboard";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to copy to clipboard");
            StatusText = "Failed to copy to clipboard";
            
            Application.Current?.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    "Failed to copy to clipboard", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            });
        }
    }

    [RelayCommand]
    private void ToggleView()
    {
        IsGridView = !IsGridView;
        StatusText = IsGridView ? "Grid view" : "Tile view";
    }

    [RelayCommand]
    private async Task ShowTodayAsync()
    {
        try
        {
            FilterFromDate = DateTime.Today;
            FilterToDate = DateTime.Today.AddDays(1);
            await ApplyFilterAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to show today's items");
            StatusText = "Error applying filter";
        }
    }

    [RelayCommand]
    private async Task ShowLast24HoursAsync()
    {
        try
        {
            FilterFromDate = DateTime.Now.AddHours(-24);
            FilterToDate = null;
            await ApplyFilterAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to show last 24 hours");
            StatusText = "Error applying filter";
        }
    }

    [RelayCommand]
    private async Task ShowImagesAsync()
    {
        try
        {
            FilterType = ClipboardType.Image;
            FilterFromDate = null;
            FilterToDate = null;
            await ApplyFilterAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to show images");
            StatusText = "Error applying filter";
        }
    }

    [RelayCommand]
    private async Task ShowTextAsync()
    {
        try
        {
            FilterType = ClipboardType.Text;
            FilterFromDate = null;
            FilterToDate = null;
            await ApplyFilterAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to show text items");
            StatusText = "Error applying filter";
        }
    }

    [RelayCommand]
    private async Task ShowPinnedAsync()
    {
        try
        {
            FilterPinned = true;
            FilterType = null;
            FilterFromDate = null;
            FilterToDate = null;
            await ApplyFilterAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to show pinned items");
            StatusText = "Error applying filter";
        }
    }

    private void OnClipboardItemAdded(object? sender, ClipboardItem item)
    {
        if (item == null) return;

        try
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Items.Insert(0, item);
                TotalCount = Items.Count;
                StatusText = $"New item added: {item.Type}";
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to add clipboard item to UI");
        }
    }
}
