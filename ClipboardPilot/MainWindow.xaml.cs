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
        StateChanged += OnStateChanged;
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
            
            // 초기 최대화/복원 버튼 상태 설정
            UpdateMaximizeRestoreButton();
            
            _viewModel.StatusText = "클립보드 파일럿이 시작되었습니다!";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"초기화 오류: {ex.Message}", "오류", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        UpdateMaximizeRestoreButton();
    }

    private void UpdateMaximizeRestoreButton()
    {
        if (MaximizeRestoreButton != null)
        {
            if (WindowState == WindowState.Maximized)
            {
                // Restore icon (E923)
                MaximizeRestoreButton.Content = "\uE923";
                MaximizeRestoreButton.ToolTip = "복원 (Restore)";
            }
            else
            {
                // Maximize icon (E922)
                MaximizeRestoreButton.Content = "\uE922";
                MaximizeRestoreButton.ToolTip = "최대화 (Maximize)";
            }
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
            MessageBox.Show($"단축키 등록 실패: {ex.Message}\n\n일부 단축키가 작동하지 않을 수 있습니다.", "경고", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ShowMiniPanel()
    {
        MessageBox.Show("미니 패널 기능은 곧 추가됩니다!", "알림", 
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
            // 더블클릭으로 최대화/복원
            Maximize_Click(sender, e);
        }
        else
        {
            // 드래그로 이동
            try
            {
                DragMove();
            }
            catch
            {
                // DragMove는 마우스 버튼이 눌린 상태에서만 작동
            }
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
        }
        else
        {
            WindowState = WindowState.Maximized;
        }
    }

    #endregion

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "클립보드 파일럿을 종료하시겠습니까?", 
            "종료 확인", 
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
            case "전체":
                _viewModel.FilterType = null;
                break;
            case "텍스트":
                _viewModel.FilterType = ClipboardType.Text;
                break;
            case "이미지":
                _viewModel.FilterType = ClipboardType.Image;
                break;
            case "HTML":
                _viewModel.FilterType = ClipboardType.Html;
                break;
            case "파일 목록":
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

        if (content == "전체")
        {
            _viewModel.FilterLabel = null;
        }
        else
        {
            var labelMap = new Dictionary<string, ColorLabel>
            {
                { "빨강", ColorLabel.Red },
                { "주황", ColorLabel.Orange },
                { "노랑", ColorLabel.Yellow },
                { "초록", ColorLabel.Green },
                { "파랑", ColorLabel.Blue },
                { "보라", ColorLabel.Purple }
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
        var helpMessage = @"클립보드 파일럿 사용법

주요 기능:
• 자동으로 클립보드 내용을 저장합니다
• 텍스트, 이미지, HTML, 파일 목록을 지원합니다
• 검색 및 필터링으로 빠르게 찾을 수 있습니다
• 중요한 항목을 고정할 수 있습니다

검색:
• 상단 검색창에 키워드를 입력하세요
• Enter 키로 검색을 실행합니다
• Esc 키로 검색을 초기화합니다

필터:
• 좌측 패널에서 유형별, 날짜별로 필터링하세요
• 빠른 필터 버튼으로 자주 사용하는 필터를 적용하세요

고정:
• 항목을 선택하고 우클릭 > 고정/해제
• 고정된 항목은 항상 상단에 표시됩니다

단축키는 '도움말 > 단축키'에서 확인하세요!";

        MessageBox.Show(helpMessage, "사용법", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ShowShortcuts_Click(object sender, RoutedEventArgs e)
    {
        var shortcutsMessage = @"클립보드 파일럿 단축키

전역 단축키:
• Ctrl+Shift+V : 미니 패널 열기
• Ctrl+Alt+V : 이전 항목 붙여넣기
• Ctrl+Alt+1~9 : 즐겨찾기 항목 붙여넣기

프로그램 내 단축키:
• Ctrl+F : 검색 포커스
• Ctrl+D : 선택 항목 삭제
• Ctrl+P : 선택 항목 고정/해제
• F5 : 새로고침
• Esc : 필터 초기화

마우스:
• 더블클릭 : 항목 복사
• 우클릭 : 컨텍스트 메뉴

팁: 설정에서 단축키를 변경할 수 있습니다!";

        MessageBox.Show(shortcutsMessage, "단축키", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
