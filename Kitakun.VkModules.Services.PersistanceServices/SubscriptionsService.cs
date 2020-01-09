namespace Kitakun.VkModules.Services.PersistanceServices
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;

    using Kitakun.VkModules.Persistance;
    using Kitakun.VkModules.Services.Abstractions;

    public class SubscriptionsService : ISubscriptionsService
    {
        private readonly IVkDbContext _dbcontext;

        public SubscriptionsService(IVkDbContext context)
        {
            _dbcontext = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<bool> GroupHasSubscription(long groupId)
#if RELEASE
             => _dbcontext
                .Subscriptions
                .AsNoTracking()
                .AnyAsync(a => a.GroupId == groupId
                    && (a.To == null || a.To > DateTime.UtcNow));
#endif
#if DEBUG
            => Task.FromResult(true);
#endif
    }
}
