using Postgirl.Presentation.ViewModels;
using Postgirl.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Postgirl.Presentation.Views;

public partial class EditConfigurationWindow : Window
{
    public EditConfigurationWindow(ConfigurationService configurationService)
    {
        InitializeComponent();
        DataContext = new EditConfigurationViewModel(configurationService);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is EditConfigurationViewModel viewModel)
        {
            viewModel.SaveChanges();
            Close();
        }
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is EditConfigurationViewModel viewModel)
        {
            viewModel.ResetChanges();
        }
    }

    private void IntegerTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        var proposedText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
            .Insert(textBox.SelectionStart, e.Text);

        e.Handled = !IsValidIntegerText(proposedText);
    }

    private void IntegerTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            e.CancelCommand();
            return;
        }

        if (!e.DataObject.GetDataPresent(typeof(string)))
        {
            e.CancelCommand();
            return;
        }

        var pastedText = (string)e.DataObject.GetData(typeof(string));
        var proposedText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
            .Insert(textBox.SelectionStart, pastedText);

        if (!IsValidIntegerText(proposedText))
        {
            e.CancelCommand();
        }
    }

    private static bool IsValidIntegerText(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "-")
        {
            return true;
        }

        return int.TryParse(value, out _);
    }
}
