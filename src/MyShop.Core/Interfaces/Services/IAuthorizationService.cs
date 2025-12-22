using MyShop.Core.Models;

namespace MyShop.Core.Interfaces.Services
{
    public interface IAuthorizationService
    {
        bool IsAuthorized(UserRole requiredRole);
        bool CanManageUsers();
        bool CanManageProducts();
        bool CanViewReports();
        bool CanModifyOrders();
    }
}
