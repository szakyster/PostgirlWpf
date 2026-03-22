using Postgirl.Common;
using Postgirl.Domain.Variables;

namespace Postgirl.Presentation.ViewModels;

public class VariableItemViewModel : BaseViewModel
{
    private string _key;
    private string _value;
    private bool _isDuplicate;

    public VariableItemViewModel(VariableEntry entry)
    {
        Entry = entry;
        _key = entry.Key;
        _value = entry.Value;
    }

    internal VariableEntry Entry { get; }

    public string Key
    {
        get => _key;
        set
        {
            value = value.Trim();
            if (!VariableKeyValidator.IsValid(value ?? string.Empty))
            {
                OnPropertyChanged(); // force UI revert to current valid value
                return;
            }

            if (SetProperty(ref _key, value!))
                Entry.Key = _key;
        }
    }

    public string Value
    {
        get => _value;
        set
        {
            if (SetProperty(ref _value, value ?? string.Empty))
                Entry.Value = _value;
        }
    }

    public bool IsDuplicate
    {
        get => _isDuplicate;
        set => SetProperty(ref _isDuplicate, value);
    }
}
