using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;
using Windows.Storage;
using System.Text.Json;

namespace MyShop.Core.Services
{
    public class SessionManager : ISessionManager
    {
        private const string TokenKey = "AuthToken";
        private const string UserKey = "CurrentUser";
        private readonly ApplicationDataContainer _localSettings;

        public SessionManager()
        {
            _localSettings = ApplicationData.Current.LocalSettings;
            LoadSession();
        }

        public User? CurrentUser { get; set; }
        public string? Token { get; set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

        public void ClearSession()
        {
            Token = null;
            CurrentUser = null;
            _localSettings.Values.Remove(TokenKey);
            _localSettings.Values.Remove(UserKey);
        }

        private void LoadSession()
        {
            if (_localSettings.Values.TryGetValue(TokenKey, out object? tokenValue))
            {
                Token = tokenValue as string;
            }

            if (_localSettings.Values.TryGetValue(UserKey, out object? userValue) && userValue is string userJson)
            {
                try
                {
                    CurrentUser = JsonSerializer.Deserialize<User>(userJson);
                }
                catch
                {
                    CurrentUser = null;
                }
            }
        }

        public void SaveSession(string token, User user)
        {
            Token = token;
            CurrentUser = user;
            _localSettings.Values[TokenKey] = token;
            _localSettings.Values[UserKey] = JsonSerializer.Serialize(user);
        }
    }
}
