using Microsoft.Extensions.DependencyInjection;
using SmartStorePOS.ViewModels;
using System.ComponentModel;

namespace SmartStorePOS.Services
{
    public interface INavigationService : INotifyPropertyChanged
    {
        ViewModelBase CurrentViewModel { get; }
        void NavigateTo<T>() where T : ViewModelBase;
        T GetViewModel<T>() where T : ViewModelBase;
    }

    public class NavigationService : INavigationService
    {
        private ViewModelBase _currentViewModel;
        private readonly IServiceProvider _serviceProvider;

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void NavigateTo<T>() where T : ViewModelBase
        {
            var viewModel = GetViewModel<T>();
            CurrentViewModel = viewModel;
        }

        public T GetViewModel<T>() where T : ViewModelBase
        {
            return _serviceProvider.GetRequiredService<T>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
