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
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private bool _rememberMe;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand(async _ => await ExecuteLoginAsync(), _ => CanLogin());
        }

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    OnPropertyChanged(nameof(HasError));
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        // Property for XAML binding to control error message visibility
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ICommand LoginCommand { get; }

        public event EventHandler? LoginSuccessful;

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsLoading;
        }

        private async Task ExecuteLoginAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var user = await _authService.LoginAsync(Username, Password);

                if (user != null)
                {
                    // Save credentials if RememberMe is checked
                    if (RememberMe)
                    {
                        // TODO: Save to secure storage
                        // await _authService.SaveCredentialsAsync(Username, Password);
                    }

                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}