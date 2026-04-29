using Postgirl.Domain.SavedRequests;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Postgirl.Services;

public class SavedRequestService
{
    public ObservableCollection<SavedRequestEntry> Items { get; }
        = new();

    public void Add(SavedRequestEntry entry)
    {
        Items.Add(entry);
    }

    public void Remove(SavedRequestEntry entry)
    {
        Items.Remove(entry);
    }

    public List<SavedRequestEntry> Export() => Items.ToList();

    public void Import(IEnumerable<SavedRequestEntry> entries)
    {
        Items.Clear();
        foreach (var e in entries)
            Items.Add(e);
    }
}