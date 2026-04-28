using Microsoft.Extensions.DependencyInjection;
using Postgirl.Domain.History;
using Postgirl.Presentation.ViewModels;
using Postgirl.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Postgirl.Presentation.Views
{
    public partial class HistoryView : UserControl
    {
        public HistoryView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnHistoryDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is HistoryViewModel vm)
            {
                vm.OpenSelectedHistoryItem();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyGroupingConfiguration();
        }

        private void ApplyGroupingConfiguration()
        {
            if (Resources["GroupedHistoryItems"] is not CollectionViewSource groupedHistoryItems)
            {
                return;
            }

            groupedHistoryItems.GroupDescriptions.Clear();

            if (App.AppHost == null)
            {
                return;
            }

            var configurationService = App.AppHost.Services.GetRequiredService<ConfigurationService>();

            if (configurationService.GetHistoryGroupByDateEnabled())
            {
                groupedHistoryItems.GroupDescriptions.Add(
                    new PropertyGroupDescription(nameof(RequestHistoryEntry.ExecutedAtGroup)));
            }
        }
    }
}