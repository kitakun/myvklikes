using System.Reflection;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Kitakun.VkModules.Core.Domain;

namespace Kitakun.VkModules.Persistance
{
    public class VkDbContext : DbContext, IVkDbContext
    {
        public DbSet<DataCollection> DataCollections { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
           => options
                .UseNpgsql("Server=localhost;Database=myvklikes;Port=5432;User Id=postgres;Password=f5432yx5o;");

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder
                .ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        public Task SaveChangesAsync() => base.SaveChangesAsync();
    }
}
