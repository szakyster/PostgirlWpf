using Postgirl.Common;
using Postgirl.Domain.Variables;
using Postgirl.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Postgirl.Presentation.ViewModels;

public class VariablesViewModel : BaseViewModel
{
    public ObservableCollection<VariableItemViewModel> Items { get; } = new();

    public VariablesViewModel(VariablesService variablesService)
    {
        foreach (var entry in variablesService.Items)
        {
            AddItem(new VariableItemViewModel(entry));
        }

        Items.CollectionChanged += OnItemsCollectionChanged;
        RefreshDuplicates();

        AddCommand = new RelayCommand(() =>
        {
            var entry = new VariableEntry("variable");
            variablesService.Add(entry);
            Items.Add(new VariableItemViewModel(entry));
        });

        DeleteCommand = new RelayCommand<VariableItemViewModel>(vm =>
        {
            if (vm == null) return;
            variablesService.Remove(vm.Entry);
            Items.Remove(vm);
        });
    }

    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }

    private void AddItem(VariableItemViewModel vm)
    {
        vm.PropertyChanged += OnItemPropertyChanged;
        Items.Add(vm);
    }

    private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (VariableItemViewModel vm in e.NewItems)
            {
                vm.PropertyChanged += OnItemPropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (VariableItemViewModel vm in e.OldItems)
            {
                vm.PropertyChanged -= OnItemPropertyChanged;

            }
        }

        RefreshDuplicates();
    }

    private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VariableItemViewModel.Key))
        {
            RefreshDuplicates();
        }
    }

    private void RefreshDuplicates()
    {
        var duplicateKeys = Items
            .Where(vm => !string.IsNullOrEmpty(vm.Key))
            .GroupBy(vm => vm.Key, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var vm in Items)
        {
            vm.IsDuplicate = duplicateKeys.Contains(vm.Key);
        }
    }
}
