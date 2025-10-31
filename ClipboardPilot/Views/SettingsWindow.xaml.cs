using DevExpress.Xpf.Core;
using ClipboardPilot.ViewModels;
using System.Windows;

namespace ClipboardPilot.Views;

public partial class SettingsWindow : ThemedWindow
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
