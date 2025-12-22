using MyShop.App.ViewModels.Base;
using MyShop.Core.Interfaces.Services;
using System.Windows.Input;

namespace MyShop.App.ViewModels
{
    public class ConfigViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private string _serverUrl;
        private string _databaseName;

        public ConfigViewModel(IConfigService configService)
        {
            _configService = configService;
            _serverUrl = _configService.GetServerUrl();
            _databaseName = _configService.GetDatabaseName();
            
            SaveCommand = new RelayCommand(_ => ExecuteSave());
            ResetCommand = new RelayCommand(_ => ExecuteReset());
        }

        public string ServerUrl
        {
            get => _serverUrl;
            set => SetProperty(ref _serverUrl, value);
        }

        public string DatabaseName
        {
            get => _databaseName;
            set => SetProperty(ref _databaseName, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand ResetCommand { get; }

        private void ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(ServerUrl))
            {
                ErrorMessage = "Server URL cannot be empty.";
                return;
            }

            _configService.SaveServerUrl(ServerUrl);
            _configService.SaveDatabaseName(DatabaseName);
            ErrorMessage = "Settings saved successfully.";
        }

        private void ExecuteReset()
        {
            ServerUrl = "http://localhost:4000/graphql";
            DatabaseName = "myshop_db";
        }
    }
}
