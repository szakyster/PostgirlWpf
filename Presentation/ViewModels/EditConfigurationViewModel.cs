using Postgirl.Domain.Configuration;
using Postgirl.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Postgirl.Presentation.ViewModels;

public class EditConfigurationViewModel
{
    private readonly ConfigurationService _configurationService;
    private readonly Dictionary<string, string> _originalValuesByKey;

    public EditConfigurationViewModel(ConfigurationService configurationService)
    {
        _configurationService = configurationService;
        _originalValuesByKey = configurationService.Items
            .ToDictionary(entry => entry.Key, entry => entry.Value);

        Items = new ObservableCollection<ConfigurationItemViewModel>(
            configurationService.Items.Select(entry => new ConfigurationItemViewModel(entry)));
    }

    public string WindowTitle => "Edit Configuration";

    public ObservableCollection<ConfigurationItemViewModel> Items { get; }

    public void SaveChanges()
    {
        foreach (var item in Items)
        {
            if (!CanSave(item))
            {
                continue;
            }

            _configurationService.SetValue(item.Key, item.Value);
        }
    }

    public void ResetChanges()
    {
        foreach (var item in Items)
        {
            if (_originalValuesByKey.TryGetValue(item.Key, out var originalValue))
            {
                item.ApplyRawValue(originalValue);
            }
        }
    }

    private static bool CanSave(ConfigurationItemViewModel item)
    {
        return item.ValueType switch
        {
            ConfigurationValueType.String => true,
            ConfigurationValueType.Integer => int.TryParse(item.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            ConfigurationValueType.Boolean => bool.TryParse(item.Value, out _),
            _ => false
        };
    }
}
