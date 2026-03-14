using Postgirl.Common;
using Postgirl.Domain.Http;
using System;
using System.Windows.Input;

namespace Postgirl.Presentation.ViewModels
{
    public class RequestParameterItemViewModel : BaseViewModel
    {
        private readonly RequestParameter _parameter;
        private readonly Action<RequestParameterItemViewModel> _removeCallback;

        public RequestParameterItemViewModel(RequestParameter parameter, Action<RequestParameterItemViewModel> removeCallback)
        {
            _parameter = parameter;
            _removeCallback = removeCallback;

            RemoveCommand = new RelayCommand(Remove);
        }

        public ICommand RemoveCommand { get; }

        public string Key
        {
            get => _parameter.Key;
            set
            {
                if (_parameter.Key != value)
                {
                    _parameter.Key = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Value
        {
            get => _parameter.Value;
            set
            {
                if (_parameter.Value != value)
                {
                    _parameter.Value = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEnabled
        {
            get => _parameter.IsEnabled;
            set
            {
                if (_parameter.IsEnabled != value)
                {
                    _parameter.IsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool CanEdit => true;

        private void Remove()
        {
            _removeCallback(this);
        }

        public bool HasValidKey()
            => !string.IsNullOrWhiteSpace(Key);

        public RequestParameter Domain => _parameter;
    }
}
