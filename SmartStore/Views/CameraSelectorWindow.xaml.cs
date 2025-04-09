using SmartStorePOS.ViewModels;
using System.Windows;

namespace SmartStorePOS.Views
{
    /// <summary>
    /// Interaction logic for CameraSelectorWindow.xaml
    /// </summary>
    public partial class CameraSelectorWindow : Window
    {
        private readonly CameraSelectorViewModel _viewModel;
        public CameraSelectorWindow()
        {
            InitializeComponent();
            _viewModel = new CameraSelectorViewModel();
            DataContext = _viewModel;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
