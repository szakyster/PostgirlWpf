using Postgirl.Common;
using Postgirl.Domain.History;
using Postgirl.Services;
using System.Collections.ObjectModel;

namespace Postgirl.Presentation.ViewModels
{
    public class HistoryViewModel(HistoryService _historyService, MainViewModel mainViewModel) : BaseViewModel
    {

        public ObservableCollection<RequestHistoryEntry> HistoryItems
            => _historyService.Items;

#nullable enable
        public RequestHistoryEntry? SelectedHistoryItem { get; set; }

        public void OpenSelectedHistoryItem()
        {
            if (SelectedHistoryItem == null)
                return;

            mainViewModel.OpenHistoryEntry(SelectedHistoryItem);
        }
    }
}
