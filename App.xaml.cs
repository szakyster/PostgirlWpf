using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Postgirl.Domain.Persistence;
using Postgirl.Presentation.ViewModels;
using Postgirl.Presentation.Views;
using Postgirl.Services;
using Postgirl.Services.Execution;

namespace Postgirl
{
    public partial class App
    {

        public static IHost AppHost { get; private set; } = null!;

        protected override async void OnStartup(StartupEventArgs e)
        {
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
                    var mainWindow = new MainWindow(null);
                    mainWindow.Show();
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {

            var storage = AppHost.Services.GetRequiredService<StorageService>();
            var history = AppHost.Services.GetRequiredService<HistoryService>();
            var saved = AppHost.Services.GetRequiredService<SavedRequestService>();
            var variables = AppHost.Services.GetRequiredService<VariablesService>();
            var mainViewModel = AppHost.Services.GetRequiredService<MainViewModel>();

            try
            {
                var state = new AppState
                {
                    History = history.Export(),
                    SavedRequests = saved.Export(),
                    Variables = variables.Export(),
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
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            //services
            services.AddSingleton<HttpExecutor>();
            services.AddSingleton<IHttpPipeline>(sp => new HttpPipeline(sp.GetRequiredService<HttpExecutor>()));
            services.AddSingleton<IHttpExecutor>(sp => sp.GetRequiredService<IHttpPipeline>());
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
            var storage = AppHost.Services.GetRequiredService<StorageService>();
            var history = AppHost.Services.GetRequiredService<HistoryService>();
            var saved = AppHost.Services.GetRequiredService<SavedRequestService>();
            var variables = AppHost.Services.GetRequiredService<VariablesService>();

            var state = await storage.LoadAsync();

            history.Import(state.History);
            saved.Import(state.SavedRequests);
            variables.Import(state.Variables);

            if (variables.Items.Count == 0)
            {
                var testVariables = new[]
                {
                    new Domain.Variables.VariableEntry { Key = "base_url",        Value = "https://api.example.com" },
                    new Domain.Variables.VariableEntry { Key = "api_version",     Value = "v2" },
                    new Domain.Variables.VariableEntry { Key = "api_key",         Value = "sk-test-abc123xyz" },
                    new Domain.Variables.VariableEntry { Key = "auth_token",      Value = "Bearer eyJhbGciOiJIUzI1NiJ9..." },
                    new Domain.Variables.VariableEntry { Key = "tenant_id",       Value = "acme-corp" },
                    new Domain.Variables.VariableEntry { Key = "user_id",         Value = "usr_98765" },
                    new Domain.Variables.VariableEntry { Key = "timeout_seconds", Value = "30" },
                    new Domain.Variables.VariableEntry { Key = "page_size",       Value = "25" },
                    new Domain.Variables.VariableEntry { Key = "environment",     Value = "staging" },
                    new Domain.Variables.VariableEntry { Key = "region",          Value = "eu-west-1" },
                };

                foreach (var v in testVariables)
                    variables.Add(v);
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
