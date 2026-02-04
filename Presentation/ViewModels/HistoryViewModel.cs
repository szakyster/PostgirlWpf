using Postgirl.Common;
using System.Collections.ObjectModel;
using Postgirl.Domain.History;
using Postgirl.Services;

namespace Postgirl.Presentation.ViewModels
{
    public class HistoryViewModel (HistoryService _historyService, MainViewModel mainViewModel) : BaseViewModel
    {

        public ObservableCollection<RequestHistoryEntry> HistoryItems
            => _historyService.Items;

        public RequestHistoryEntry? SelectedHistoryItem { get; set; }

        public void OpenSelectedHistoryItem()
        {
            if (SelectedHistoryItem == null)
                return;

            mainViewModel.OpenHistoryEntry(SelectedHistoryItem);
        }
    }
}
