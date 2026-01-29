
using Postgirl.Services;
using System.Windows;
using Postgirl.Presentation.ViewModels;

namespace Postgirl
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(new HttpService());
        }
    }
}
