using MyShop.App.ViewModels.Base;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;
using System.Windows.Input;

namespace MyShop.App.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly IAuthorizationService _authorizationService;

        public ShellViewModel(IAuthService authService, IAuthorizationService authorizationService)
        {
            _authService = authService;
            _authorizationService = authorizationService;
            LogoutCommand = new RelayCommand(_ => ExecuteLogout());
            AdminPanelCommand = new RelayCommand(_ => ExecuteOpenAdminPanel());
        }

        public User? CurrentUser => _authService.CurrentUser;
        
        public UserRole UserRole => CurrentUser?.Role ?? UserRole.STAFF;

        public bool IsAdmin => _authorizationService.IsAuthorized(UserRole.ADMIN);

        public ICommand LogoutCommand { get; }
        public ICommand AdminPanelCommand { get; }

        public event System.Action? LogoutRequested;

        private void ExecuteLogout()
        {
            _authService.LogoutAsync();
            LogoutRequested?.Invoke();
        }

        private void ExecuteOpenAdminPanel()
        {
            // Demonstration of an action that only an Admin should trigger
            System.Diagnostics.Debug.WriteLine("Admin Panel opened.");
        }
    }
}
