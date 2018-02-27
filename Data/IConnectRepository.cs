using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectApi.Models;

namespace connect_api.Data
{
    public interface IConnectRepository
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveAll();

        Task<IEnumerable<User>> GetUsers();
        Task<User> GetUser(int id);
    }
}