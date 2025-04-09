using Microsoft.Extensions.DependencyInjection;
using SmartStorePOS.Helpers;
using SmartStorePOS.Services;
using SmartStorePOS.ViewModels;
using SmartStorePOS.Views;
using System.Configuration;
using System.Windows;

namespace SmartStorePOS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;
        public ServiceProvider ServiceProvider
        {
            get => serviceProvider;
        }

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Register services
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IApiService, ApiService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Register ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<OrderViewModel>();

            // Register Views
            services.AddTransient<LoginView>();
            services.AddTransient<MainView>();
            services.AddTransient<OrderView>();

            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var navigationService = serviceProvider.GetRequiredService<INavigationService>();

            string savedToken = string.Empty;

            if (ConfigurationManager.AppSettings["AppToken"] != null)
            {
                savedToken = ConfigurationManager.AppSettings["AppToken"];
            }
            else
            {
                savedToken = SecureStorageHelper.GetToken();
            }

            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();

            if (!string.IsNullOrEmpty(savedToken))
            {
                var apiService = serviceProvider.GetRequiredService<IApiService>();
                apiService.SetToken(savedToken);

                navigationService.NavigateTo<MainViewModel>();
            }
            else
            {
                navigationService.NavigateTo<LoginViewModel>();
            }

            mainWindow.Show();
        }
    }
}
