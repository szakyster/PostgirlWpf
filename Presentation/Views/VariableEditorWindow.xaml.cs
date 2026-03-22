using System.Windows;

namespace Postgirl.Presentation.Views;

public partial class VariableEditorWindow : Window
{
    public string EditedValue { get; private set; }

    public VariableEditorWindow(string key, string value)
    {
        InitializeComponent();
        Title = $"Edit: {key}";
        EditorTextBox.Text = value;
        EditedValue = value;
        EditorTextBox.CaretIndex = value.Length;
    }

    private void OnApply(object sender, RoutedEventArgs e)
    {
        EditedValue = EditorTextBox.Text;
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
