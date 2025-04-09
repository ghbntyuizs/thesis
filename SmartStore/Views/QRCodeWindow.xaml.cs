using SmartStorePOS.ViewModels;
using System.Windows;

namespace SmartStorePOS.Views
{
    /// <summary>
    /// Interaction logic for QRCodeWindow.xaml
    /// </summary>
    public partial class QRCodeWindow : Window
    {
        private readonly QRCodeViewModel _viewModel;

        public QRCodeWindow()
        {
            InitializeComponent();

            _viewModel = new QRCodeViewModel();
            _viewModel.CloseAction = Close;

            DataContext = _viewModel;
        }

        public QRCodeWindow(string initialText) : this()
        {
            _viewModel.QRCodeText = initialText;
            // Generate QR code immediately if text is provided
            if (!string.IsNullOrWhiteSpace(initialText))
            {
                _viewModel.GenerateQRCodeCommand.Execute(null);
            }
        }
    }
}
