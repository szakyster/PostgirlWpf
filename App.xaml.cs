using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Postgirl.Domain.Persistence;
using Postgirl.Presentation.ViewModels;
using Postgirl.Presentation.Views;
using Postgirl.Services;
using Postgirl.Services.Execution;
using Postgirl.Services.Execution.Steps;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Postgirl
{
    public partial class App
    {
        public static IHost AppHost { get; private set; } = null!;

        private readonly LockfileService _lockfileService = new();

        protected override async void OnStartup(StartupEventArgs e)
        {
            if (!_lockfileService.TryAcquire())
            {
                MessageBox.Show(
                    "A Postgirl már fut egy másik ablakban.\n\nEgyszerre csak egy példány indítható el.",
                    "Postgirl – már fut",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                Shutdown();
                return;
            }

            try
            {
                AppHost = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) => { ConfigureServices(services); })
                    .Build();

                await AppHost.StartAsync();
                await InitializeApplicationAsync();
                base.OnStartup(e);
            }
            catch (Exception)
            {
                // If initialization fails, we still want to show the main window so the user can see the error message
                var mainWindow = new MainWindow(null, null);
                mainWindow.Show();
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            var projectService = AppHost.Services.GetRequiredService<ProjectService>();
            var mainViewModel = AppHost.Services.GetRequiredService<MainViewModel>();
            var configuration = AppHost.Services.GetRequiredService<ConfigurationService>();
            var storage = AppHost.Services.GetRequiredService<StorageService>();

            try
            {
                // Persist opened documents and sidebar state back into the active project
                projectService.ActiveProject.OpenedDocuments = mainViewModel.ExportOpenedDocuments();
                projectService.ActiveProject.ActiveSidebarPanel = mainViewModel.ActiveSidebarPanel;
                projectService.SaveActiveProject();

                // Persist global configuration
                var globalState = new Postgirl.Domain.Persistence.AppState
                {
                    Configuration = configuration.Export()
                };
                storage.SaveConfiguration(globalState);

                if (AppHost != null)
                {
                    await AppHost.StopAsync();
                    AppHost.Dispose();
                }
                base.OnExit(e);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                _lockfileService.Dispose();
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            //services
            services.AddSingleton<HttpExecutor>();
            services.AddSingleton<IHttpPipeline>(sp =>
            {
                var pipeline = new HttpPipeline(sp.GetRequiredService<HttpExecutor>());
                pipeline.Register(new VariableSubstitutionStep(
                    sp.GetRequiredService<ConfigurationService>(),
                    sp.GetRequiredService<VariablesService>()));
                return pipeline;
            });
            services.AddSingleton<IHttpExecutor>(sp => sp.GetRequiredService<IHttpPipeline>());
            services.AddSingleton<ConfigurationService>();
            services.AddSingleton<HistoryService>();
            services.AddSingleton<SavedRequestService>();
            services.AddSingleton<VariablesService>();
            services.AddSingleton<StorageService>();
            services.AddSingleton<ProjectService>();

            //WMs
            services.AddSingleton<MainViewModel>();

            //Views
            services.AddSingleton<MainWindow>();
        }

        private async Task InitializeApplicationAsync()
        {
            var storageService = AppHost.Services.GetRequiredService<StorageService>();
            var configurationService = AppHost.Services.GetRequiredService<ConfigurationService>();
            var projectService = AppHost.Services.GetRequiredService<ProjectService>();

            // Load global configuration first
            var globalState = await storageService.LoadConfigurationAsync();
            configurationService.Import(globalState.Configuration);

            // Migrate legacy state if needed, then load active project into services
            await projectService.InitializeAsync();

            // MainViewModel resolved AFTER all imports so ViewModels initialize from populated services
            var mainViewModel = AppHost.Services.GetRequiredService<MainViewModel>();

            var openedDocs = projectService.ActiveProject.OpenedDocuments;
            if (openedDocs != null && openedDocs.Count > 0)
            {
                mainViewModel.ImportOpenedDocuments(openedDocs);
            }
            else
            {
                mainViewModel.NewTabCommand.Execute(null);
            }

            mainViewModel.ActiveSidebarPanel = projectService.ActiveProject.ActiveSidebarPanel;

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
