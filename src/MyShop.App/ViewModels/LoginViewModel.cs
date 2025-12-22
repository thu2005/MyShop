using MyShop.App.ViewModels.Base;
using MyShop.Core.Interfaces.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyShop.App.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _rememberMe;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand(async _ => await ExecuteLoginAsync(), _ => CanLogin());
            OpenConfigCommand = new RelayCommand(_ => ExecuteOpenConfig());
        }

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand OpenConfigCommand { get; }

        public event EventHandler? LoginSuccessful;
        public event EventHandler? OpenConfigRequested;

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsBusy;
        }

        private async Task ExecuteLoginAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var user = await _authService.LoginAsync(Username, Password);

                if (user != null)
                {
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ExecuteOpenConfig()
        {
            OpenConfigRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}