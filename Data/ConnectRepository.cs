using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConnectApi.Data;
using ConnectApi.Models;
using Microsoft.EntityFrameworkCore;

namespace connect_api.Data
{
    public class ConnectRepository : IConnectRepository
    {
        private readonly DataContext _context;
        public ConnectRepository(DataContext context) => this._context = context;
        public void Add<T>(T entity) where T : class => _context.Add(entity);
        public void Delete<T>(T entity) where T : class => _context.Remove(entity);
        public async Task<User> GetUser(int id)
        {
            var user = await this._context.User.Include(p => p.Photos)
            .Include(e => e.Education).Include(proj => proj.Projects)
            .Include(s => s.Skills).Include(w => w.WorkExperiences)
            .FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }
        public async Task<IEnumerable<User>> GetUsers()
        {
            var users = await this._context.User.Include(p => p.Photos)
            .Include(s => s.Skills)
            .ToListAsync();

            return users;
        }
        public async Task<bool> SaveAll() => await this._context.SaveChangesAsync() > 0;
        public Task<Photo> GetPhoto(int id)
        {
            var photo = this._context.Photo.FirstOrDefaultAsync(pic => pic.Id == id);
            return photo;
        }
        public Task<Photo> GetMainPhoto(int userId)
        {
            return this._context.Photo.Where(userPhoto => userPhoto.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        }
    }
}