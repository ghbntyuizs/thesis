using SmartStorePOS.Services;
using SmartStorePOS.ViewModels;
using System.Windows;

namespace SmartStorePOS.Views
{
    public partial class MembershipCardWindow : Window
    {
        private readonly MembershipCardViewModel _viewModel;


        public MembershipCardWindow(IDialogService dialogService, IApiService apiService)
        {
            InitializeComponent();
            _viewModel = new MembershipCardViewModel(dialogService, apiService);
            DataContext = _viewModel;
            this.Name = nameof(MembershipCardWindow);

            // Clean up resources when window is closed
            //Closed += (s, e) => _viewModel.Dispose();
        }
    }
}
