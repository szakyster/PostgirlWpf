using Postgirl.Presentation.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Postgirl.Presentation.Views
{
    public partial class HistoryView : UserControl
    {
        public HistoryView()
        {
            InitializeComponent();
        }

        private void OnHistoryDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is HistoryViewModel vm)
            {
                vm.OpenSelectedHistoryItem();
            }
        }
    }
}