namespace Kitakun.VkModules.Web.Controllers
{
    using System;
    using System.Threading.Tasks;

    using Hangfire;
    using Hangfire.Server;

    using Microsoft.EntityFrameworkCore;

    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Persistance;
    using Kitakun.VkModules.Web.Components;
    using Kitakun.VkModules.Core.Extensions;

    public class BackgroundUpdater
    {
        private readonly IGroupLikesService _groupLikeService;
        private readonly IVkDbContext _dbContext;
        private readonly ITop100Service _top100Service;

        public BackgroundUpdater(
            IGroupLikesService groupLikeService,
            IVkDbContext dbContext,
            ITop100Service top100Service)
        {
            _groupLikeService = groupLikeService ?? throw new ArgumentNullException(nameof(groupLikeService));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _top100Service = top100Service ?? throw new ArgumentNullException(nameof(top100Service));
        }

        public async Task Run(long groupId, PerformContext hfContext)
        {
            var curDate = DateTime.UtcNow;
            var setting = default(Core.Domain.GroupSettings);

            using (var locker = await KeyLocker<long>.LockAsync(groupId))
            {
                setting = await _dbContext.GroupSettings
                    .FirstOrDefaultAsync(f => f.GroupId == groupId);

                if (setting == null)
                    return;

                var currentJobId = hfContext.BackgroundJob.Id;

                // prevent multiple same jobs
                if (!string.IsNullOrEmpty(setting.LastRunnedJobId)
                    && currentJobId != setting.LastRunnedJobId)
                    return;

                setting.LastRunnedJobId = currentJobId;
                await _dbContext.SaveChangesAsync();
            }

            var hasSub = await _dbContext.Subscriptions
                .AnyAsync(a => a.GroupId == groupId && a.From <= curDate && a.To >= curDate);

            if (!hasSub)
            {
                // Disable auto-updating
                setting.BackgroundJobType = Core.Domain.BackgroundUpdaterType.Undefined;
                RecurringJob.RemoveIfExists(setting.RecuringBackgroundJobId);
                setting.RecuringBackgroundJobId = null;
                setting.LastRunnedJobId = null;

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                var modelTask = _top100Service.LoadTop100(groupId, true);
                var vkApiTask = _groupLikeService.GetApi(setting.GroupAppToken);

                await Task.WhenAll(modelTask, vkApiTask);

                setting.LastRunnedJobId = null;
                await _dbContext.SaveChangesAsync();

                var model = modelTask.Result;
                var vk = vkApiTask.Result;

                switch (setting.BackgroundJobType)
                {
                    case Core.Domain.BackgroundUpdaterType.Top3:
                        var top3VkCode = Top100BestLikersComponent.GenerateCodeFromModelForApi(model);
                        await vk.AppWidgets.UpdateAsync(top3VkCode, "list");
                        break;
                }
            }
        }
    }
}
