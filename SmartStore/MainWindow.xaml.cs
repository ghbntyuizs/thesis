using SmartStorePOS.Services;
using System.Windows;

namespace SmartStorePOS
{
    /// <summary>
    /// Interaction logic for MainWindowMainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly INavigationService _navigationService;

        public MainWindow(INavigationService navigationService)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _navigationService = navigationService;
            DataContext = _navigationService;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //_navigationService.NavigateTo<LoginViewModel>();
        }
    }
}
