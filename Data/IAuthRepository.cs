using System.Threading.Tasks;
using ConnectApi.Models;

namespace ConnectApi.Data
{
    public interface IAuthRepository
    {
        Task<User> Register(User user, string password);
        Task<User> Login(string email, string password);
        Task<bool> UsernameExists(string username);
        Task<bool> EmailExists(string email);
    }
}