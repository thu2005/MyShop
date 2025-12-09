using MyShop.Core.Models;
using System.Threading.Tasks;

namespace MyShop.Core.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> ValidateCredentialsAsync(string username, string password);
    }
}