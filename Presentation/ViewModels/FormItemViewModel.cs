using System;
using System.Windows.Input;
using Postgirl.Common;
using Postgirl.Domain.Http.Body;

namespace Postgirl.Presentation.ViewModels;

public class FormItemViewModel : BaseViewModel
{
    private readonly Action<FormItemViewModel> _removeAction;
    private readonly FormUrlEncodedItem _model;

    public FormItemViewModel(
        FormUrlEncodedItem model,
        Action<FormItemViewModel> removeAction)
    {
        _model = model;
        _removeAction = removeAction;

        RemoveCommand = new RelayCommand(() => _removeAction(this));
    }

    public string Key
    {
        get => _model.Key;
        set
        {
            if (_model.Key == value) return;
            _model.Key = value;
            OnPropertyChanged();
        }
    }

    public string Value
    {
        get => _model.Value;
        set
        {
            if (_model.Value == value) return;
            _model.Value = value;
            OnPropertyChanged();
        }
    }

    public ICommand RemoveCommand { get; }

    public FormUrlEncodedItem ToDomain()
        => _model.Copy();
}