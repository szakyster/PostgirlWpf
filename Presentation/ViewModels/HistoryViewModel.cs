using Postgirl.Common;
using System.Collections.ObjectModel;
using Postgirl.Domain.History;
using Postgirl.Services;

namespace Postgirl.Presentation.ViewModels
{
    public class HistoryViewModel (HistoryService _historyService) : BaseViewModel
    {

        public ObservableCollection<RequestHistoryEntry> HistoryItems
            => _historyService.Items;

    }
}
