using System.Windows.Controls;
using RemoteDesktopApp.ViewModels;

namespace RemoteDesktopApp.Views;

public partial class HeaderBar : UserControl
{
    public HeaderBar()
    {
        InitializeComponent();
    }
    
    public HeaderBar(HeaderBarViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}