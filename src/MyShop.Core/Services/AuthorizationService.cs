using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;

namespace MyShop.Core.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IAuthService _authService;

        public AuthorizationService(IAuthService authService)
        {
            _authService = authService;
        }

        public bool IsAuthorized(UserRole requiredRole)
        {
            var user = _authService.CurrentUser;
            if (user == null) return false;

            // Admin can do everything
            if (user.Role == UserRole.ADMIN) return true;

            // Manager can do everything except what is strictly Admin
            if (user.Role == UserRole.MANAGER)
            {
                return requiredRole != UserRole.ADMIN;
            }

            // Staff can only do Staff things
            return user.Role == requiredRole;
        }

        public bool CanManageUsers() => IsAuthorized(UserRole.ADMIN);
        
        public bool CanManageProducts() => IsAuthorized(UserRole.MANAGER);
        
        public bool CanViewReports() => IsAuthorized(UserRole.MANAGER);
        
        public bool CanModifyOrders() => IsAuthorized(UserRole.STAFF);
    }
}
