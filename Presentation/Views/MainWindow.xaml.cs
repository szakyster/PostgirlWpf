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
        }
    }
}
