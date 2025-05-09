using SmartStorePOS.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartStorePOS.Views
{
    /// <summary>
    /// Interaction logic for OrderView.xaml
    /// </summary>
    public partial class OrderView : UserControl
    {
        public OrderView()
        {
            InitializeComponent();
            Loaded += OrderView_Loaded;
        }

        private void OrderView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is OrderViewModel orderViewModel)
            {
                orderViewModel.FocusMainWindow = () =>
                {
                    Keyboard.Focus(BtnQRCodePayment);
                    Application.Current.MainWindow.Activate();
                    Application.Current.MainWindow.Focus();
                };
            }
        }
    }
}
