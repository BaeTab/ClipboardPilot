using DevExpress.Xpf.Core;
using ClipboardPilot.ViewModels;
using ClipboardPilot.Services;
using ClipboardPilot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace ClipboardPilot;

public partial class MainWindow : ThemedWindow
{
    private readonly MainViewModel _viewModel;
    private readonly ClipboardWatcher _clipboardWatcher;
    private readonly HotkeyService _hotkeyService;
    private readonly SettingsService _settingsService;

    public MainWindow(
        MainViewModel viewModel,
        ClipboardWatcher clipboardWatcher,
        HotkeyService hotkeyService,
        SettingsService settingsService)
    {
        InitializeComponent();
        
        _viewModel = viewModel;
        _clipboardWatcher = clipboardWatcher;
        _hotkeyService = hotkeyService;
        _settingsService = settingsService;
        
        DataContext = _viewModel;
        
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.InitializeAsync();
            _clipboardWatcher.Start(this);
            
            var helper = new WindowInteropHelper(this);
            _hotkeyService.Initialize(helper.Handle);
            
            RegisterGlobalHotkeys();
            
            _viewModel.StatusText = "Ŭ������ ���Ϸ��� ����Ǿ����ϴ�!";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"�ʱ�ȭ ����: {ex.Message}", "����", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RegisterGlobalHotkeys()
    {
        try
        {
            _hotkeyService.RegisterHotkey(
                _settingsService.Settings.Hotkeys.ShowMiniPanel,
                () => ShowMiniPanel());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"����Ű ��� ����: {ex.Message}\n\n�Ϻ� ����Ű�� �۵����� ���� �� �ֽ��ϴ�.", "���", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ShowMiniPanel()
    {
        MessageBox.Show("�̴� �г� ����� �� �߰��˴ϴ�!", "����", 
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _clipboardWatcher.Stop();
        _hotkeyService.Dispose();
    }

    #region Title Bar Event Handlers

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }
        else
        {
            DragMove();
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
            WindowState = WindowState.Normal;
        else
            WindowState = WindowState.Maximized;
    }

    #endregion

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Ŭ������ ���Ϸ��� �����Ͻðڽ��ϱ�?", 
            "���� Ȯ��", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Question);
            
        if (result == MessageBoxResult.Yes)
        {
            Close();
        }
    }

    #region Filter Event Handlers

    private void FilterToday_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.ShowTodayCommand.CanExecute(null))
        {
            _viewModel.ShowTodayCommand.Execute(null);
        }
    }

    private void FilterLast24Hours_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.ShowLast24HoursCommand.CanExecute(null))
        {
            _viewModel.ShowLast24HoursCommand.Execute(null);
        }
    }

    private void FilterImages_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.ShowImagesCommand.CanExecute(null))
        {
            _viewModel.ShowImagesCommand.Execute(null);
        }
    }

    private void FilterText_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.ShowTextCommand.CanExecute(null))
        {
            _viewModel.ShowTextCommand.Execute(null);
        }
    }

    private void FilterPinned_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.ShowPinnedCommand.CanExecute(null))
        {
            _viewModel.ShowPinnedCommand.Execute(null);
        }
    }

    private void TypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel == null || sender is not ComboBox comboBox)
            return;

        if (comboBox.SelectedItem is not ComboBoxItem selectedItem)
            return;

        var content = selectedItem.Content?.ToString();
        if (content == null)
            return;

        switch (content)
        {
            case "��ü":
                _viewModel.FilterType = null;
                break;
            case "�ؽ�Ʈ":
                _viewModel.FilterType = ClipboardType.Text;
                break;
            case "�̹���":
                _viewModel.FilterType = ClipboardType.Image;
                break;
            case "HTML":
                _viewModel.FilterType = ClipboardType.Html;
                break;
            case "���� ���":
                _viewModel.FilterType = ClipboardType.FileList;
                break;
        }
    }

    private void LabelFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel == null || sender is not ComboBox comboBox)
            return;

        if (comboBox.SelectedItem is not ComboBoxItem selectedItem)
            return;

        var content = selectedItem.Content?.ToString();
        if (content == null)
            return;

        if (content == "��ü")
        {
            _viewModel.FilterLabel = null;
        }
        else
        {
            var labelMap = new Dictionary<string, ColorLabel>
            {
                { "����", ColorLabel.Red },
                { "��Ȳ", ColorLabel.Orange },
                { "���", ColorLabel.Yellow },
                { "�ʷ�", ColorLabel.Green },
                { "�Ķ�", ColorLabel.Blue },
                { "����", ColorLabel.Purple }
            };

            if (labelMap.TryGetValue(content, out var label))
            {
                _viewModel.FilterLabel = label;
            }
        }
    }

    private void PinnedOnly_Changed(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null || sender is not CheckBox checkBox)
            return;

        _viewModel.FilterPinned = checkBox.IsChecked ?? false;
    }

    private void ApplyFilters_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.ApplyFilterCommand.CanExecute(null))
        {
            _viewModel.ApplyFilterCommand.Execute(null);
        }
    }

    #endregion

    private void ShowHelp_Click(object sender, RoutedEventArgs e)
    {
        var helpMessage = @"Ŭ������ ���Ϸ� ����

�ֿ� ���:
? �ڵ����� Ŭ������ ������ �����մϴ�
? �ؽ�Ʈ, �̹���, HTML, ���� ����� �����մϴ�
? �˻� �� ���͸����� ������ ã�� �� �ֽ��ϴ�
? �߿��� �׸��� ������ �� �ֽ��ϴ�

�˻�:
? ��� �˻�â�� Ű���带 �Է��ϼ���
? Enter Ű�� �˻��� �����մϴ�
? Esc Ű�� �˻��� �ʱ�ȭ�մϴ�

����:
? ���� �гο��� ������, ��¥���� ���͸��ϼ���
? ���� ���� ��ư���� ���� ����ϴ� ���͸� �����ϼ���

����:
? �׸��� �����ϰ� ��Ŭ�� > ����/����
? ������ �׸��� �׻� ��ܿ� ǥ�õ˴ϴ�

����Ű�� '���� > ����Ű'���� Ȯ���ϼ���!";

        MessageBox.Show(helpMessage, "����", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ShowShortcuts_Click(object sender, RoutedEventArgs e)
    {
        var shortcutsMessage = @"Ŭ������ ���Ϸ� ����Ű

���� ����Ű:
? Ctrl+Shift+V : �̴� �г� ����
? Ctrl+Alt+V : ���� �׸� �ٿ��ֱ�
? Ctrl+Alt+1~9 : ���ã�� �׸� �ٿ��ֱ�

���α׷� �� ����Ű:
? Ctrl+F : �˻� ��Ŀ��
? Ctrl+D : ���� �׸� ����
? Ctrl+P : ���� �׸� ����/����
? F5 : ���ΰ�ħ
? Esc : ���� �ʱ�ȭ

���콺:
? ����Ŭ�� : �׸� ����
? ��Ŭ�� : ���ؽ�Ʈ �޴�

��: �������� ����Ű�� ������ �� �ֽ��ϴ�!";

        MessageBox.Show(shortcutsMessage, "����Ű", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
