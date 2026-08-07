using System.Windows;
using System.Windows.Input;

namespace Postgirl.Presentation.Views
{
    public partial class ProjectNameInputWindow : Window
    {
        public static readonly DependencyProperty PromptProperty =
            DependencyProperty.Register(nameof(Prompt), typeof(string), typeof(ProjectNameInputWindow),
                new PropertyMetadata("Enter project name:"));

        public string Prompt
        {
            get => (string)GetValue(PromptProperty);
            set => SetValue(PromptProperty, value);
        }

        public string ProjectName { get; private set; } = string.Empty;

        private readonly System.Collections.Generic.IEnumerable<string> _existingNames;

        public ProjectNameInputWindow(
            string prompt,
            System.Collections.Generic.IEnumerable<string> existingNames,
            string initialValue = "")
        {
            InitializeComponent();
            Prompt = prompt;
            _existingNames = existingNames;
            NameTextBox.Text = initialValue;
            Loaded += (_, _) =>
            {
                NameTextBox.Focus();
                NameTextBox.SelectAll();
            };
        }

        private bool Validate()
        {
            var name = NameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError("Name cannot be empty.");
                return false;
            }

            foreach (var existing in _existingNames)
            {
                if (string.Equals(existing, name, System.StringComparison.OrdinalIgnoreCase))
                {
                    ShowError($"A project named '{name}' already exists.");
                    return false;
                }
            }

            return true;
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate())
                return;

            ProjectName = NameTextBox.Text.Trim();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OkButton_Click(sender, e);
            else if (e.Key == Key.Escape)
                CancelButton_Click(sender, e);
        }
    }
}
