using Microsoft.Extensions.DependencyInjection;
using SmartStorePOS.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SmartStorePOS.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Hiển thị message box thông báo tính năng đang được phát triển
        /// </summary>
        /// <param name="featureName">Tên tính năng đang được phát triển</param>
        protected void ShowFeatureInDevelopmentMessage(string featureName)
        {
            var serviceProvider = (Application.Current as App).ServiceProvider;
            var messageBoxService = serviceProvider.GetRequiredService<IDialogService>();
            messageBoxService.ShowInfoDialog($"Đang phát triển", $"Tính năng {featureName} đang được phát triển");
        }
    }
}
