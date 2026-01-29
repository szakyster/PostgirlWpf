using Postgirl.Common;
using Postgirl.Domain.Http;
using System;
using System.Windows.Input;

namespace Postgirl.Presentation.ViewModels
{
    public class RequestHeaderItemViewModel : BaseViewModel
    {
        private readonly RequestHeader _header;
        private readonly Action<RequestHeaderItemViewModel> _removeCallback;

        public RequestHeaderItemViewModel(RequestHeader header, Action<RequestHeaderItemViewModel> removeCallback)
        {
            _header = header; 
            _removeCallback = removeCallback;

            RemoveCommand = new RelayCommand(Remove, CanRemove);
        }

        public ICommand RemoveCommand { get; }

        public string Key {
            get => _header.Key;
            set
            {
                if (_header.Key != value)
                {
                    _header.Key = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Value
        {
            get => _header.Value;
            set
            {
                if (_header.Value != value)
                {
                    _header.Value = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSystem => _header.IsSystem;

        public bool CanEdit => !_header.IsSystem;
        
        public bool IsUser => !_header.IsSystem;

        public bool IsEnabled
        {
            get => _header.IsEnabled;
            set
            {
                if (IsSystem)
                {
                    value = true;
                }

                if (_header.IsEnabled != value)
                {
                    _header.IsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool CanRemove() => !_header.IsSystem;

        private void Remove()
        {
            _removeCallback(this);
        }

        public bool HasValidKey()
            => !string.IsNullOrWhiteSpace(Key);

        public RequestHeader Domain => _header;
    }
}
