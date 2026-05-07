using Postgirl.Services;
using Postgirl.Presentation.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Postgirl.Presentation.Views
{
    public partial class MainWindow : Window
    {
        private readonly ConfigurationService _configurationService;
        private readonly Expander[] _sidebarExpanders;

        public MainWindow(MainViewModel vm, ConfigurationService configurationService)
        {
            InitializeComponent();
            _configurationService = configurationService;
            DataContext = vm;
            Closing += OnClosing;
            Loaded += OnLoaded;

            _sidebarExpanders = [SavedExpander, HistoryExpander, VariablesExpander];
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel vm)
            {
                return;
            }

            foreach (var expander in _sidebarExpanders)
            {
                expander.IsExpanded = expander.Name == vm.ActiveSidebarPanel;
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is not Expander expanded)
            {
                return;
            }

            if (_sidebarExpanders is null)
            {
                return;
            }

            foreach (var expander in _sidebarExpanders)
            {
                if (expander != expanded)
                {
                    expander.IsExpanded = false;
                }
            }

            if (DataContext is MainViewModel vm)
            {
                vm.ActiveSidebarPanel = expanded.Name;
            }
        }

        private void OptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_configurationService == null)
            {
                return;
            }

            var editConfigurationWindow = new EditConfigurationWindow(_configurationService)
            {
                Owner = this
            };

            editConfigurationWindow.ShowDialog();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.CancelAllRequests();
            }
        }
    }
}
