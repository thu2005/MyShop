using MyShop.Core.Models;

namespace MyShop.Core.Interfaces.Services
{
    public interface ISessionManager
    {
        User? CurrentUser { get; set; }
        string? Token { get; set; }
        bool IsAuthenticated { get; }
        void ClearSession();
    }
}
