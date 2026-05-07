using Postgirl.Common;
using Postgirl.Domain.Configuration;
using System;
using System.Globalization;

namespace Postgirl.Presentation.ViewModels;

public class ConfigurationItemViewModel : BaseViewModel
{
    private readonly string _originalValue;
    private string _textValue;
    private bool _boolValue;

    public ConfigurationItemViewModel(ConfigurationEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        Key = entry.Key;
        DisplayName = entry.DisplayName;
        Description = entry.Description;
        ValueType = entry.ValueType;
        _textValue = entry.Value;
        _boolValue = bool.TryParse(entry.Value, out var boolValue) && boolValue;
        _originalValue = NormalizeValueForComparison(entry.Value);
    }

    public string Key { get; }

    public string DisplayName { get; }

    public string Description { get; }

    public ConfigurationValueType ValueType { get; }

    public bool IsStringValueType => ValueType == ConfigurationValueType.String;

    public bool IsIntegerValueType => ValueType == ConfigurationValueType.Integer;

    public bool IsBooleanValueType => ValueType == ConfigurationValueType.Boolean;

    public string TextValue
    {
        get => _textValue;
        set
        {
            if (SetProperty(ref _textValue, value))
            {
                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(IsModified));
            }
        }
    }

    public bool BoolValue
    {
        get => _boolValue;
        set
        {
            if (SetProperty(ref _boolValue, value))
            {
                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(IsModified));
            }
        }
    }

    public string Value => IsBooleanValueType
        ? (_boolValue ? bool.TrueString.ToLowerInvariant() : bool.FalseString.ToLowerInvariant())
        : _textValue;

    public bool IsModified => !string.Equals(_originalValue, NormalizeValueForComparison(Value), StringComparison.Ordinal);

    public bool IsValid => ValueType switch
    {
        ConfigurationValueType.String => true,
        ConfigurationValueType.Integer => string.IsNullOrWhiteSpace(_textValue) || _textValue == "-" || int.TryParse(_textValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
        ConfigurationValueType.Boolean => true,
        _ => false
    };

    public void ApplyRawValue(string value)
    {
        if (IsBooleanValueType)
        {
            if (bool.TryParse(value, out var boolValue))
            {
                BoolValue = boolValue;
            }

            return;
        }

        TextValue = value;
    }

    private string NormalizeValueForComparison(string value)
    {
        if (IsBooleanValueType)
        {
            if (bool.TryParse(value, out var boolValue))
            {
                return boolValue ? bool.TrueString.ToLowerInvariant() : bool.FalseString.ToLowerInvariant();
            }

            return bool.FalseString.ToLowerInvariant();
        }

        return value ?? string.Empty;
    }
}
