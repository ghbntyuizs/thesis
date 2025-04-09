using SmartStorePOS.Helpers;
using SmartStorePOS.Models;
using SmartStorePOS.Services;
using SmartStorePOS.Views;
using System.Windows.Input;

namespace SmartStorePOS.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        private bool _isLoading;
        private string _errorMessage;
        private bool _isCapturing;
        private string _deviceId = new CreateOrderRequest().DeviceId;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsCapturing
        {
            get => _isCapturing;
            set => SetProperty(ref _isCapturing, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string DeviceId
        {
            get => _deviceId;
            set => SetProperty(ref _deviceId, value);
        }

        //public ICommand CaptureImageCommand { get; }
        public ICommand CreateOrderCommand { get; }
        public ICommand HistoryCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand HelpCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel(IApiService apiService, INavigationService navigationService)
        {
            _navigationService = navigationService;

            CreateOrderCommand = new RelayCommand(async _ => { await CreateOrderAsync(); }, _ => !IsLoading);
            HistoryCommand = new RelayCommand(_ => { ShowFeatureInDevelopmentMessage("Xem lịch sử"); });
            SettingsCommand = new RelayCommand(_ => { Setting(); });
            HelpCommand = new RelayCommand(_ => { ShowFeatureInDevelopmentMessage("Trợ giúp"); });
            LogoutCommand = new RelayCommand(_ => Logout());
        }

        private async Task CreateOrderAsync()
        {
            _navigationService.NavigateTo<OrderViewModel>();
        }

        private void Setting()
        {
            new CameraSelectorWindow().ShowDialog();
        }

        private void Logout()
        {
            SecureStorageHelper.ClearToken();
            _navigationService.NavigateTo<LoginViewModel>();
        }
    }
}
