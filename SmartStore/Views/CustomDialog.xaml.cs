using SmartStorePOS.ViewModels;
using System.Windows;

namespace SmartStorePOS.Views
{
    /// <summary>
    /// Interaction logic for CustomDialog.xaml
    /// </summary>
    public partial class CustomDialog : Window
    {
        private readonly DialogViewModel _viewModel;

        public CustomDialog(DialogType dialogType, string title, string message)
        {
            InitializeComponent();

            _viewModel = new DialogViewModel(dialogType, title, message);
            _viewModel.CloseAction = result =>
            {
                DialogResult = result;
                Close();
            };

            DataContext = _viewModel;
        }
    }
}
