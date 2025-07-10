using System.Windows.Controls;
using RemoteDesktopApp.ViewModels;

namespace RemoteDesktopApp.Views;

public partial class TabContainer : UserControl
{
    public TabContainer()
    {
        InitializeComponent();
    }
    
    public TabContainer(TabContainerViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}