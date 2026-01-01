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
        private readonly GraphQLService _graphQLService;

        public SessionManager(GraphQLService graphQLService)
        {
            _graphQLService = graphQLService;
            _localSettings = ApplicationData.Current.LocalSettings;
            LoadSession();
        }

        public User? CurrentUser { get; set; }
        public string? Token { get; set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
        public bool IsSessionPersisted => _localSettings.Values.ContainsKey(TokenKey);

        public void ClearSession()
        {
            Token = null;
            CurrentUser = null;
            _localSettings.Values.Remove(TokenKey);
            _localSettings.Values.Remove(UserKey);
            _localSettings.Values.Remove("LastOpenedPage"); // Clear last opened page on logout

            _graphQLService.SetAuthToken(null);
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

            if (IsAuthenticated)
            {
                _graphQLService.SetAuthToken(Token);
            }
        }

        public void SaveSession(string token, User user, bool rememberMe)
        {
            Token = token;
            CurrentUser = user;

            if (rememberMe)
            {
                _localSettings.Values[TokenKey] = token;
                _localSettings.Values[UserKey] = JsonSerializer.Serialize(user);
            }

            _graphQLService.SetAuthToken(token);
        }
    }
}