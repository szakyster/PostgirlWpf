using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Postgirl.Domain.Persistence;
using Postgirl.Presentation.ViewModels;
using Postgirl.Presentation.Views;
using Postgirl.Services;
using System.Collections.Generic;
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

            var storage = AppHost.Services.GetRequiredService<StorageService>();
            var history = AppHost.Services.GetRequiredService<HistoryService>();
            var saved = AppHost.Services.GetRequiredService<SavedRequestService>();
            var variables = AppHost.Services.GetRequiredService<VariablesService>();
            var configuration = AppHost.Services.GetRequiredService<ConfigurationService>();
            var mainViewModel = AppHost.Services.GetRequiredService<MainViewModel>();

            try
            {
                var state = new AppState
                {
                    History = configuration.GetStorageKeepHistoryBetweenSessions() ? history.Export() : [],
                    SavedRequests = saved.Export(),
                    Variables = variables.Export(),
                    Configuration = configuration.Export(),
                    OpenedDocuments = mainViewModel.ExportOpenedDocuments(),
                    ActiveSidebarPanel = mainViewModel.ActiveSidebarPanel
                };

                storage.SaveAsync(state);
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

            //WMs
            services.AddSingleton<MainViewModel>();

            //Views
            services.AddSingleton<MainWindow>();
        }

        private async Task InitializeApplicationAsync()
        {
            var storageService = AppHost.Services.GetRequiredService<StorageService>();
            var historyService = AppHost.Services.GetRequiredService<HistoryService>();
            var savedService = AppHost.Services.GetRequiredService<SavedRequestService>();
            var variablesService = AppHost.Services.GetRequiredService<VariablesService>();
            var configurationService = AppHost.Services.GetRequiredService<ConfigurationService>();

            var state = await storageService.LoadAsync();

            configurationService.Import(state.Configuration);

            if (configurationService.GetStorageKeepHistoryBetweenSessions())
            {
                historyService.Import(state.History);
            }

            savedService.Import(state.SavedRequests);
            variablesService.Import(state.Variables);

            if (variablesService.Items.Count == 0)
            {
                variablesService.SeedDefaults();
            }

            // mainViewModel resolved AFTER all imports so VariablesViewModel initializes from populated service
            var mainViewModel = AppHost.Services.GetRequiredService<MainViewModel>();

            // Import opened documents or add new empty document if none exist
            if (state.OpenedDocuments != null && state.OpenedDocuments.Count > 0)
            {
                mainViewModel.ImportOpenedDocuments(state.OpenedDocuments);
            }
            else
            {
                // If no saved documents, create a new empty one
                mainViewModel.NewTabCommand.Execute(null);
            }

            mainViewModel.ActiveSidebarPanel = state.ActiveSidebarPanel;

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
