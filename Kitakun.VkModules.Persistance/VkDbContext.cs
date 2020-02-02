using System.Reflection;
using System.Threading.Tasks;

#if DEBUG
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
#endif
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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
        private readonly IConfiguration _configuration;

        public DbSet<DataCollection> DataCollections { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<GroupSettings> GroupSettings { get; set; }

#if !(MIGRATION)
        public VkDbContext(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
        }
#endif

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options
#if DEBUG
                .UseLoggerFactory(MyLoggerFactory)
#endif
#if MIGRATION
                .UseNpgsql("User ID=migrator;Password=migrator;Host=localhost;Port=5432;Database=myvklikes;Pooling=true;");
#else
                .UseNpgsql(_configuration.GetConnectionString("Persistance"));
#endif

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder
                .ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        public Task SaveChangesAsync() => base.SaveChangesAsync();
    }
}
