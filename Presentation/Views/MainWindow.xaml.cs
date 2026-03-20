using System.Windows;
using System.Windows.Controls;
using Postgirl.Presentation.ViewModels;
using Postgirl.Services;

namespace Postgirl.Presentation.Views
{
    public partial class MainWindow : Window
    {
        private readonly Expander[] _sidebarExpanders;

        public MainWindow(MainViewModel vm)
        {
            InitializeComponent(); 
            DataContext = vm;
            Closing += OnClosing;

            _sidebarExpanders = [SavedExpander, HistoryExpander];
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is not Expander expanded) return;
            if (_sidebarExpanders is null) return;

            foreach (var expander in _sidebarExpanders)
            {
                if (expander != expanded)
                    expander.IsExpanded = false;
            }
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
