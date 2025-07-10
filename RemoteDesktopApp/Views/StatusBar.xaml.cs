using System.Windows.Controls;
using RemoteDesktopApp.ViewModels;

namespace RemoteDesktopApp.Views;

public partial class StatusBar : UserControl
{
    public StatusBar()
    {
        InitializeComponent();
    }
    
    public StatusBar(StatusBarViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}