namespace Kitakun.VkModules.Web.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Persistance;
    using Kitakun.VkModules.Web.WebModels;
    using Kitakun.VkModules.Web.Components;

    //[EnableCors(WebConstants.AllCorsName)]
    public class BackgroundUpdater// : Controller
    {
        private const string AppToken = "924ddd86924ddd86924ddd86df922afcec9924d924ddd86c9b8b1155b39d3337c8f8840";
        private readonly IGroupLikesService _groupLikeService;
        private readonly IVkDbContext _dbContext;

        public BackgroundUpdater(
            IGroupLikesService groupLikeService,
            IVkDbContext dbContext)
        {
            _groupLikeService = groupLikeService ?? throw new ArgumentNullException(nameof(groupLikeService));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task Run([FromQuery] long groupId)
        {
            var curDate = DateTime.UtcNow;

            var hasSub = await _dbContext.Subscriptions.AnyAsync(a => a.GroupId == groupId && a.From <= curDate && a.To >= curDate);
            if (!hasSub)
            {
                //TODO log
                return;
            }

            var setting = await _dbContext.GroupSettings.FirstOrDefaultAsync(f => f.GroupId == groupId);

            var actualGroupId = setting.ReverseGroup
                ? groupId * -1
                : groupId;

            var currentDate = DateTime.Now;
            var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var data = await _groupLikeService.LoadAllLikesForCommunityPostsAsync(AppToken, actualGroupId, firstDayOfMonth, lastDayOfMonth);
            var top100 = data
                .OrderByDescending(x => x.Value)
                .Select(s => s.Key)
                .Take(100)
                .ToArray();
            var model = new Top100BestLikersModel
            {
                IsAdmin = false,
                Likes = data,
                Top100 = top100,
                TopUsersTitleText = setting.TopLikersHeaderMessage
            };

            var vk = await _groupLikeService.GetApi(setting.GroupAppToken);
            var code = Top100BestLikersComponent.GenerateCodeFromModelForApi(model);
            await vk.AppWidgets.UpdateAsync(code, "list");
        }
    }
}
