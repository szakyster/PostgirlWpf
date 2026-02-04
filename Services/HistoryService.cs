using System.Collections.ObjectModel;
using Postgirl.Domain.History;

namespace Postgirl.Services;

public class HistoryService
{
    public ObservableCollection<RequestHistoryEntry> Items { get; }
        = new();

    public void Add(RequestHistoryEntry entry)
    {
        Items.Insert(0, entry); // legfrissebb felül
    }
}