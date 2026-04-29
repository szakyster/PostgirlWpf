using Postgirl.Presentation.ViewModels;
using Postgirl.Services;
using System.Windows;

namespace Postgirl.Presentation.Views;

public partial class EditConfigurationWindow : Window
{
    public EditConfigurationWindow(ConfigurationService configurationService)
    {
        InitializeComponent();
        DataContext = new EditConfigurationViewModel(configurationService);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
