using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using RemoteDesktopApp.ViewModels;

namespace RemoteDesktopApp.Views;

public partial class SettingsPanel : UserControl
{
    public SettingsPanel()
    {
        InitializeComponent();
        Loaded += SettingsPanel_Loaded;
    }
    
    public SettingsPanel(SettingsPanelViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
    
    private void SettingsPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (IsVisible)
        {
            var slideIn = (Storyboard)Resources["SlideInAnimation"];
            slideIn.Begin(this);
        }
    }
    
    private void Backdrop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Close settings panel when clicking on backdrop
        if (DataContext is SettingsPanelViewModel vm)
        {
            vm.CloseSettingsCommand?.Execute(null);
        }
    }
    
    public void ShowPanel()
    {
        Visibility = System.Windows.Visibility.Visible;
        var slideIn = (Storyboard)Resources["SlideInAnimation"];
        slideIn.Begin(this);
    }
    
    public void HidePanel()
    {
        var slideOut = (Storyboard)Resources["SlideOutAnimation"];
        slideOut.Completed += (s, e) => Visibility = System.Windows.Visibility.Collapsed;
        slideOut.Begin(this);
    }
}