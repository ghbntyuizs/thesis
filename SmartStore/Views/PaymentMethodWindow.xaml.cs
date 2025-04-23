using System.Windows;

namespace SmartStorePOS.Views
{
    public partial class PaymentMethodWindow : Window
    {
        public enum PaymentMethod
        {
            None,
            QRCode,
            MembershipCard
        }

        public PaymentMethod SelectedMethod { get; private set; } = PaymentMethod.None;

        public PaymentMethodWindow()
        {
            InitializeComponent();
        }

        private void QRCodeButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedMethod = PaymentMethod.QRCode;
            DialogResult = true;
            Close();
        }

        private void MembershipCardButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedMethod = PaymentMethod.MembershipCard;
            DialogResult = true;
            Close();
        }
    }
}
