using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Kitakun.VkModules.Core.Domain;

namespace Kitakun.VkModules.Persistance
{
    public interface IVkDbContext
    {
        DbSet<DataCollection> DataCollections { get; }

        Task SaveChangesAsync();
    }
}
