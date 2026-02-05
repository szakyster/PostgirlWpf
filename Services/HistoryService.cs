using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

    public List<RequestHistoryEntry> Export()
    {
        return Items.ToList();
    }

    public void Import(IEnumerable<RequestHistoryEntry> entries)
    {
        Items.Clear();
        foreach (var e in entries)
            Items.Add(e);
    }
}