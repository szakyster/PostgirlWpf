using System.Collections.ObjectModel;
using System.Windows.Input;
using Postgirl.Common;
using Postgirl.Domain.Variables;
using Postgirl.Services;

namespace Postgirl.Presentation.ViewModels;

public class VariablesViewModel : BaseViewModel
{
    private readonly VariablesService _variablesService;

    public VariablesViewModel(VariablesService variablesService)
    {
        _variablesService = variablesService;
        AddCommand = new RelayCommand(() => _variablesService.Add(new VariableEntry()));
        DeleteCommand = new RelayCommand<VariableEntry>(entry => { if (entry != null) _variablesService.Remove(entry); });
    }

    public ObservableCollection<VariableEntry> Items => _variablesService.Items;

    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }
}
