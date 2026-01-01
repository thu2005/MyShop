using MyShop.Core.Models;

namespace MyShop.Core.Interfaces.Services
{
    public interface ISessionManager
    {
        User? CurrentUser { get; set; }
        string? Token { get; set; }
        bool IsAuthenticated { get; }
        bool IsSessionPersisted { get; } // True if user chose "Remember Me"
        void ClearSession();

        void SaveSession(string token, User user, bool rememberMe);
    }
}
