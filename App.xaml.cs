
using System;
using System.Windows;
using Postgirl.Domain.Persistence;
using Postgirl.Presentation.ViewModels;
using Postgirl.Presentation.Views;
using Postgirl.Services;

namespace Postgirl
{
    public partial class App : Application
    {
        private readonly HistoryService _historyService = new();
        private readonly StorageService _storageService = new();
        private readonly HttpService _httpService = new();

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var state = await _storageService.LoadAsync();
            _historyService.Import(state.History);

            var mainVm = new MainViewModel(_httpService, _historyService, _storageService);

            MainWindow = new MainWindow
            {
                DataContext = mainVm
            };

            MainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            try
            {
                var state = new AppState
                {
                    History = _historyService.Export()
                };

                await _storageService.SaveAsync(state);

                base.OnExit(e);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }


    }
}
