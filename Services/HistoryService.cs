using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Postgirl.Domain.History;

namespace Postgirl.Services;

public class HistoryService
{
    private readonly ConfigurationService _configurationService;

    public HistoryService(ConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    public ObservableCollection<RequestHistoryEntry> Items { get; }
        = new();

    public void Add(RequestHistoryEntry entry)
    {
        Items.Insert(0, entry); // legfrissebb felül
    }

    public List<RequestHistoryEntry> Export()
    {
        var retainedItemCount = Math.Max(0, _configurationService.GetRetainedHistoryItemCount());
        return Items.Take(retainedItemCount).ToList();
    }

    public void Import(IEnumerable<RequestHistoryEntry> entries)
    {
        Items.Clear();
        foreach (var e in entries)
        {
            Items.Add(e);
        }
    }
}