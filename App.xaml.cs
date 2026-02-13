
using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Postgirl.Domain.Persistence;
using Postgirl.Presentation.ViewModels;
using Postgirl.Presentation.Views;
using Postgirl.Services;

namespace Postgirl
{
    public partial class App : Application
    {

        public static IHost AppHost { get; private set; } = null!;

        protected override async void OnStartup(StartupEventArgs e)
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => { ConfigureServices(services); })
                .Build();

            await AppHost.StartAsync();
            await InitializeApplicationAsync();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {

            var storage = AppHost.Services.GetRequiredService<StorageService>();
            var history = AppHost.Services.GetRequiredService<HistoryService>();
            var saved = AppHost.Services.GetRequiredService<SavedRequestService>();

            try
            {
                var state = new AppState
                {
                    History = history.Export(),
                    SavedRequests = saved.Export()
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

        private void ConfigureServices(IServiceCollection services)
        {
            //services
            services.AddSingleton<IHttpExecutor, HttpExecutor>();
            services.AddSingleton<HistoryService>();
            services.AddSingleton<SavedRequestService>();
            services.AddSingleton<StorageService>();
            services.AddSingleton<HttpService>();
            
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

            var state = await storage.LoadAsync();
            history.Import(state.History);
            saved.Import(state.SavedRequests);

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
