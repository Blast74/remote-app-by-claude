using System.Windows.Controls;
using RemoteDesktopApp.ViewModels;

namespace RemoteDesktopApp.Views;

public partial class SidePanel : UserControl
{
    public SidePanel()
    {
        InitializeComponent();
    }
    
    public SidePanel(SidePanelViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}