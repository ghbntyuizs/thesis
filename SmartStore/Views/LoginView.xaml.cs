using System.Windows.Controls;

namespace SmartStorePOS.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();

            // Set up password binding
            Loaded += (sender, e) =>
            {
                if (DataContext is ViewModels.LoginViewModel viewModel)
                {
                    PasswordBox.Password = viewModel.Password;

                    PasswordBox.PasswordChanged += (s, args) =>
                    {
                        viewModel.Password = PasswordBox.Password;
                    };
                }
            };
        }
    }
}
