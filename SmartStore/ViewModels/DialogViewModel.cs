using SmartStorePOS.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SmartStorePOS.ViewModels
{
    public enum DialogType
    {
        Confirmation,
        YesNo,
        Info
    }
    public class DialogViewModel : INotifyPropertyChanged
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

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private DialogType _dialogType;
        public DialogType DialogType
        {
            get => _dialogType;
            set => SetProperty(ref _dialogType, value);
        }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand YesCommand { get; }
        public ICommand NoCommand { get; }
        public ICommand CloseCommand { get; }

        public Action<bool> CloseAction { get; set; }

        public DialogViewModel(DialogType dialogType, string title, string message)
        {
            DialogType = dialogType;
            Title = title;
            Message = message;

            OkCommand = new RelayCommand(_ =>
            {
                CloseAction?.Invoke(true);
            });

            CancelCommand = new RelayCommand(_ =>
            {
                CloseAction?.Invoke(false);
            });

            YesCommand = new RelayCommand(_ =>
            {
                CloseAction?.Invoke(true);
            });

            NoCommand = new RelayCommand(_ =>
            {
                CloseAction?.Invoke(false);
            });

            CloseCommand = new RelayCommand(_ =>
            {
                CloseAction?.Invoke(false);
            });
        }
    }
}
