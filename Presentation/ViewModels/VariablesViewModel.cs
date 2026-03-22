using System.Collections.ObjectModel;
using System.Windows.Input;
using Postgirl.Common;
using Postgirl.Domain.Variables;
using Postgirl.Services;

namespace Postgirl.Presentation.ViewModels;

public class VariablesViewModel : BaseViewModel
{
    private readonly VariablesService _variablesService;

    public ObservableCollection<VariableItemViewModel> Items { get; } = new();

    public VariablesViewModel(VariablesService variablesService)
    {
        _variablesService = variablesService;

        foreach (var entry in variablesService.Items)
            Items.Add(new VariableItemViewModel(entry));

        AddCommand = new RelayCommand(() =>
        {
            var entry = new VariableEntry("variable");
            _variablesService.Add(entry);
            Items.Add(new VariableItemViewModel(entry));
        });

        DeleteCommand = new RelayCommand<VariableItemViewModel>(vm =>
        {
            if (vm == null) return;
            _variablesService.Remove(vm.Entry);
            Items.Remove(vm);
        });
    }

    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }
}
