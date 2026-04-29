using Postgirl.Domain.Configuration;
using Postgirl.Services;
using System.Collections.ObjectModel;

namespace Postgirl.Presentation.ViewModels;

public class EditConfigurationViewModel
{
    public EditConfigurationViewModel(ConfigurationService configurationService)
    {
        Items = configurationService.Items;
    }

    public string WindowTitle => "Edit Configuration";

    public ReadOnlyCollection<ConfigurationEntry> Items { get; }
}
