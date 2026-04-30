using Postgirl.Domain.Configuration;
using Postgirl.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Postgirl.Presentation.ViewModels;

public class EditConfigurationViewModel
{
    private readonly ConfigurationService _configurationService;

    public EditConfigurationViewModel(ConfigurationService configurationService)
    {
        _configurationService = configurationService;
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
