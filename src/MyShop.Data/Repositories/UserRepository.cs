using Microsoft.EntityFrameworkCore;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using MyShop.Data.Context;
using MyShop.Data.Repositories.Base;
using System.Threading.Tasks;

namespace MyShop.Data.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(MyShopDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null) return false;

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
    }
}