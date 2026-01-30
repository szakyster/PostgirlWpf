using Postgirl.Common;
using System.Collections.ObjectModel;
using Postgirl.Domain.History;

namespace Postgirl.Presentation.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        public ObservableCollection<RequestHistoryEntry>HistoryItems { get; }
            = new();

        public HistoryViewModel()
        {
            // TEMP: demo adatok
            HistoryItems.Add(new RequestHistoryEntry
            {
                Method = "GET",
                Url = "https://api.example.com/users",
                StatusCode = 200,
                DurationMs = 124
            });

            HistoryItems.Add(new RequestHistoryEntry
            {
                Method = "POST",
                Url = "https://api.example.com/login",
                StatusCode = 401,
                DurationMs = 98,
                AuthToken = "Bearer abc123"
            });

            HistoryItems.Add(new RequestHistoryEntry
            {
                Method = "PUT",
                Url = "https://api.example.com/profile",
                StatusCode = 500,
                DurationMs = 342
            });
        }
    }
}
