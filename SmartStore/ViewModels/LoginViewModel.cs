using SmartStorePOS.Helpers;
using SmartStorePOS.Services;
using System.Windows.Input;

namespace SmartStorePOS.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IApiService _apiService;
        private readonly INavigationService _navigationService;

        private string _email = "nghiant@aistore.com";
        private string _password = "12345678";
        private bool _isLoading;
        private string _errorMessage;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(IApiService apiService, INavigationService navigationService)
        {
            _apiService = apiService;
            _navigationService = navigationService;

            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => !IsLoading);
        }

        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _apiService.LoginAsync(Email, Password);

                if (response.IsSuccess)
                {
                    SecureStorageHelper.SaveToken(response.AccessToken);

                    _navigationService.NavigateTo<MainViewModel>();
                }
                else
                {
                    ErrorMessage = response.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
