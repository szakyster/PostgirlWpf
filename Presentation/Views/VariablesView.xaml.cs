using System.Windows;
using System.Windows.Controls;
using Postgirl.Presentation.ViewModels;

namespace Postgirl.Presentation.Views;

public partial class VariablesView : UserControl
{
    public VariablesView()
    {
        InitializeComponent();
    }

    private void OnOpenEditorClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.DataContext is not VariableItemViewModel vm) return;

        var dialog = new VariableEditorWindow(vm.Key, vm.Value)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
            vm.Value = dialog.EditedValue;
    }
}

