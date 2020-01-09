using System.Reflection;
using System.Threading.Tasks;

#if DEBUG
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
#endif
using Microsoft.EntityFrameworkCore;

using Kitakun.VkModules.Core.Domain;

namespace Kitakun.VkModules.Persistance
{
    public class VkDbContext : DbContext, IVkDbContext
    {
#if DEBUG
        public static readonly ILoggerFactory MyLoggerFactory = new LoggerFactory(new[]
        {
            new ConsoleLoggerProvider((cat, lvl) =>
                cat == DbLoggerCategory.Database.Command.Name &&
                lvl == LogLevel.Information, true)
        });
#endif

        public DbSet<DataCollection> DataCollections { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options
#if DEBUG
                .UseLoggerFactory(MyLoggerFactory)
                .UseNpgsql("Server=localhost;Database=myvklikes;Port=5432;User Id=postgres;Password=keysecret;");
#elif RELEASE
                .UseNpgsql("Server=localhost;Database=myvklikes;Port=5432;User Id=liker;Password=keysecret;");
#endif


        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder
                .ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        public Task SaveChangesAsync() => base.SaveChangesAsync();
    }
}
