using Postgirl.Services;
using Postgirl.Presentation.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Postgirl.Presentation.Views
{
    public partial class MainWindow : Window
    {
        private readonly ConfigurationService _configurationService;
        private readonly MainViewModel _vm;
        private readonly Expander[] _sidebarExpanders;
        private bool _suppressProjectSwitch;

        public MainWindow(MainViewModel vm, ConfigurationService configurationService)
        {
            InitializeComponent();
            _configurationService = configurationService;
            _vm = vm;
            DataContext = vm;
            Closing += OnClosing;
            Loaded += OnLoaded;

            _sidebarExpanders = [SavedExpander, HistoryExpander, VariablesExpander];
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel vm)
                return;

            foreach (var expander in _sidebarExpanders)
            {
                expander.IsExpanded = expander.Name == vm.ActiveSidebarPanel;
            }

            SyncProjectComboBox();
        }

        private void SyncProjectComboBox()
        {
            _suppressProjectSwitch = true;
            foreach (var item in ProjectComboBox.Items)
            {
                if (item is Domain.Persistence.ProjectSummary p && p.Name == _vm.ActiveProjectName)
                {
                    ProjectComboBox.SelectedItem = item;
                    break;
                }
            }
            _suppressProjectSwitch = false;
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is not Expander expanded)
                return;

            if (_sidebarExpanders is null)
                return;

            foreach (var expander in _sidebarExpanders)
            {
                if (expander != expanded)
                    expander.IsExpanded = false;
            }

            if (DataContext is MainViewModel vm)
                vm.ActiveSidebarPanel = expanded.Name;
        }

        private void OptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_configurationService == null)
                return;

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

        private async void ProjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressProjectSwitch)
                return;

            if (ProjectComboBox.SelectedItem is not Domain.Persistence.ProjectSummary selected)
                return;

            if (selected.Name == _vm.ActiveProjectName)
                return;

            _suppressProjectSwitch = true;
            await System.Threading.Tasks.Task.Run(() => { });
            _vm.SwitchProjectCommand.Execute(selected.Id);
            _suppressProjectSwitch = false;
        }

        private void ManageProjectsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new ProjectManagementWindow(_vm)
            {
                Owner = this
            };
            window.ShowDialog();
            SyncProjectComboBox();
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.CancelAllRequests();
        }
    }
}

