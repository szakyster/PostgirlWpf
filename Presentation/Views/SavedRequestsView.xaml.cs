using Postgirl.Domain.SavedRequests;
using Postgirl.Presentation.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Postgirl.Presentation.Views
{
    public partial class SavedRequestsView: UserControl
    {
        public SavedRequestsView()
        {
            InitializeComponent();
        }
        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm &&
                ((ListBox)sender).SelectedItem is SavedRequestEntry entry)
            {
                vm.OpenSaved(entry);
            }
        }
    }
}
