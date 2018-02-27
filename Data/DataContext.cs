using ConnectApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectApi.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<Value> Values { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Education> Education { get; set; }
        public DbSet<Photo> Photo { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<Skill> Skill { get; set; }
        public DbSet<WorkExperience> WorkExperience { get; set; }
    }
}