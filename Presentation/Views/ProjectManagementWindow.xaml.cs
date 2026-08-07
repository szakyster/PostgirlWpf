using Postgirl.Domain.Persistence;
using Postgirl.Presentation.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Postgirl.Presentation.Views
{
    public partial class ProjectManagementWindow : Window
    {
        private readonly MainViewModel _vm;

        public ProjectManagementWindow(MainViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            RefreshList();
        }

        private void RefreshList()
        {
            ProjectsList.ItemsSource = null;
            ProjectsList.ItemsSource = _vm.Projects;
        }

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            var existingNames = _vm.Projects.Select(p => p.Name);
            var dialog = new ProjectNameInputWindow("New project name:", existingNames)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                _vm.CreateProjectCommand.Execute(dialog.ProjectName);
                RefreshList();
            }
        }

        private void RenameProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not ProjectSummary project)
                return;

            var existingNames = _vm.Projects
                .Where(p => p.Id != project.Id)
                .Select(p => p.Name);

            var dialog = new ProjectNameInputWindow("Rename project:", existingNames, project.Name)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                _vm.RenameProjectCommand.Execute((project.Id, dialog.ProjectName));
                RefreshList();
            }
        }

        private async void DuplicateProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not ProjectSummary project)
                return;

            var existingNames = _vm.Projects.Select(p => p.Name);
            var suggested = project.Name + " (copy)";
            var dialog = new ProjectNameInputWindow("Duplicate project as:", existingNames, suggested)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                await _vm.DuplicateProjectAsync(project.Id, dialog.ProjectName);
                RefreshList();
            }
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not ProjectSummary project)
                return;

            var result = MessageBox.Show(
                $"Delete project '{project.Name}'?\n\nThis action is permanent and all project data will be lost.",
                "Delete Project",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _vm.DeleteProjectCommand.Execute(project.Id);
                RefreshList();
            }
        }
    }
}
