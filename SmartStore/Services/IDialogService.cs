using Microsoft.Extensions.DependencyInjection;
using SmartStorePOS.ViewModels;
using SmartStorePOS.Views;
using System.Windows;

namespace SmartStorePOS.Services
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmationAsync(string title, string message);
        Task<bool> ShowYesNoDialogAsync(string title, string message);
        bool ShowYesNoDialog(string title, string message);
        Task ShowInfoDialogAsync(string title, string message);
        void ShowInfoDialog(string title, string message);
        T GetService<T>() where T : Window;
    }

    public class DialogService : IDialogService
    {
        private readonly IServiceProvider _serviceProvider;

        public DialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<bool> ShowConfirmationAsync(string title, string message)
        {
            var dialog = new CustomDialog(DialogType.Confirmation, title, message);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
            return Task.FromResult(dialog.DialogResult == true);
        }

        public Task<bool> ShowYesNoDialogAsync(string title, string message)
        {
            var dialog = new CustomDialog(DialogType.YesNo, title, message);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
            return Task.FromResult(dialog.DialogResult == true);
        }

        public Task ShowInfoDialogAsync(string title, string message)
        {
            var dialog = new CustomDialog(DialogType.Info, title, message);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
            return Task.CompletedTask;
        }

        public bool ShowYesNoDialog(string title, string message)
        {
            var dialog = new CustomDialog(DialogType.YesNo, title, message);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
            return dialog.DialogResult == true;
        }

        public void ShowInfoDialog(string title, string message)
        {
            var dialog = new CustomDialog(DialogType.Info, title, message);
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }

        public T GetService<T>() where T : Window
        {
            return (T)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(T));
        }
    }
}
