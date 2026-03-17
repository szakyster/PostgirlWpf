using System.Windows;
using Postgirl.Presentation.ViewModels;
using Postgirl.Services;

namespace Postgirl.Presentation.Views
{
    public partial class MainWindow : Window
    {

        public MainWindow(MainViewModel vm)
        {
            InitializeComponent(); 
            DataContext = vm;
            Closing += OnClosing;
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
