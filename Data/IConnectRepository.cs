using System.Threading.Tasks;
using connect_api.Helpers;
using ConnectApi.Models;

namespace connect_api.Data
{
    public interface IConnectRepository
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveAll();

        Task<PagedList<User>> GetUsers(UserPaginationParameter userPagination);
        Task<User> GetUser(int id);
        Task<Photo> GetPhoto(int id);
        Task<Photo> GetMainPhoto(int userId);
    }
}