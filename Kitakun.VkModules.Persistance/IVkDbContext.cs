namespace Kitakun.VkModules.Persistance
{
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;

    using Kitakun.VkModules.Core.Domain;

    public interface IVkDbContext
    {
        DbSet<DataCollection> DataCollections { get; }

        DbSet<GroupSettings> GroupSettings { get; }

        DbSet<Subscription> Subscriptions { get; }

        Task SaveChangesAsync();
    }
}
