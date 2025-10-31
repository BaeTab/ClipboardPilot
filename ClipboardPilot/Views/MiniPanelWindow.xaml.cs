using DevExpress.Xpf.Core;
using ClipboardPilot.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace ClipboardPilot.Views;

public partial class MiniPanelWindow : ThemedWindow
{
    private readonly MiniPanelViewModel _viewModel;

    public MiniPanelWindow(MiniPanelViewModel viewModel)
    {
        InitializeComponent();
        
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        Loaded += OnLoaded;
        
        // Esc Å°·Î ´Ý±â
        KeyDown += (s, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };
        
        // Æ÷Ä¿½º¸¦ ÀÒÀ¸¸é ´Ý±â
        Deactivated += (s, e) =>
        {
            Close();
        };
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadRecentItemsAsync();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            this.DragMove();
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
