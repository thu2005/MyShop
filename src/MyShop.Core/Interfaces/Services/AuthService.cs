using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;
using System;
using System.Threading.Tasks;

namespace MyShop.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private User? _currentUser;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User? CurrentUser => _currentUser;

        public async Task<User?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var isValid = await _userRepository.ValidateCredentialsAsync(username, password);

            if (isValid)
            {
                _currentUser = await _userRepository.GetByUsernameAsync(username);

                if (_currentUser != null)
                {
                    _currentUser.LastLoginAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(_currentUser);
                }

                return _currentUser;
            }

            return null;
        }

        public Task LogoutAsync()
        {
            _currentUser = null;
            return Task.CompletedTask;
        }

        public Task<bool> IsAuthenticatedAsync()
        {
            return Task.FromResult(_currentUser != null);
        }
    }
}