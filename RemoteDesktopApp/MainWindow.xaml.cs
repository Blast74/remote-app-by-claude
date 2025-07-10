using System.Windows;
using RemoteDesktopApp.ViewModels;

namespace RemoteDesktopApp
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}
