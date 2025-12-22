using GraphQL;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;
using System;
using System.Threading.Tasks;

namespace MyShop.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly GraphQLService _graphQLService;
        private readonly ISessionManager _sessionManager;
        private readonly IConfigService _configService;

        public AuthService(GraphQLService graphQLService, ISessionManager sessionManager, IConfigService configService)
        {
            _graphQLService = graphQLService;
            _sessionManager = sessionManager;
            _configService = configService;
        }

        public User? CurrentUser => _sessionManager.CurrentUser;
        public string? Token => _sessionManager.Token;

        public async Task<User?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var request = new GraphQLRequest
            {
                Query = @"
                    mutation Login($input: LoginInput!) {
                        login(input: $input) {
                            token
                            refreshToken
                            user {
                                id
                                username
                                email
                                role
                                isActive
                            }
                        }
                    }",
                Variables = new { input = new { username, password } }
            };

            try
            {
                System.Diagnostics.Debug.WriteLine($"Attempting login for user: {username} at {_graphQLService.Client.Options.EndPoint}");
                
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(15));
                var responseTask = _graphQLService.Client.SendMutationAsync<LoginResponse>(request, cts.Token);
                
                var response = await responseTask;

                System.Diagnostics.Debug.WriteLine("Login response received.");

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    foreach (var error in response.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"GraphQL Error: {error.Message}");
                    }
                    return null;
                }

                if (response.Data?.Login != null)
                {
                    var token = response.Data.Login.Token;
                    var user = response.Data.Login.User;
                    
                    if (user != null)
                    {
                        // Use reflection or a dedicated method in SessionManager if it exists
                        // For now, I'll cast SessionManager or use the public SaveSession I just added
                        if (_sessionManager is SessionManager s)
                        {
                            s.SaveSession(token, user);
                        }
                        
                        System.Diagnostics.Debug.WriteLine("Login successful and session saved.");
                        return user;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("Login failed: Data is null.");
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Login request timed out (15s).");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Error: {ex.InnerException.Message}");
                }
            }

            return null;
        }

        public Task LogoutAsync()
        {
            _sessionManager.ClearSession();
            return Task.CompletedTask;
        }

        public Task<bool> IsAuthenticatedAsync()
        {
            return Task.FromResult(_sessionManager.IsAuthenticated);
        }

        private class LoginResponse
        {
            public LoginResult? Login { get; set; }
        }

        private class LoginResult
        {
            public string Token { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public User? User { get; set; }
        }
    }
}