
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
        private readonly SavedRequestService _savedRequestService = new();

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var state = await _storageService.LoadAsync();
            _historyService.Import(state.History);
            _savedRequestService.Import(state.SavedRequests);

            var mainVm = new MainViewModel(_httpService, _historyService, _storageService, _savedRequestService);

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
                    History = _historyService.Export(),
                    SavedRequests = _savedRequestService.Export()
                };

                _storageService.SaveAsync(state);

                base.OnExit(e);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }


    }
}
