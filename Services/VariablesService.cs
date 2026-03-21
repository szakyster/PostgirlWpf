using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Postgirl.Domain.Variables;

namespace Postgirl.Services;

public class VariablesService
{
    public ObservableCollection<VariableEntry> Items { get; } = new();

    public void Add(VariableEntry entry) => Items.Add(entry);

    public void Remove(VariableEntry entry) => Items.Remove(entry);

    public List<VariableEntry> Export() => Items.ToList();

    public void Import(IEnumerable<VariableEntry> entries)
    {
        Items.Clear();
        foreach (var e in entries)
            Items.Add(e);
    }
}
