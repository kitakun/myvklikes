namespace Kitakun.VkModules.Services.GroupLikeService
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    using Kitakun.VkModules.Core.Models;
    using Kitakun.VkModules.Persistance;
    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Core.Extensions;

    public class Top100Service : ITop100Service
    {
        private readonly IVkDbContext _dbContext;
        private readonly IDataCollectionsService _dataCollectionsService;
        private readonly IGroupLikesService _likeService;
        private readonly IConfiguration _configuration;

        public Top100Service(
            IVkDbContext dbContext,
            IDataCollectionsService dataCollectionsService,
            IGroupLikesService likesService,
            IConfiguration configuration)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dataCollectionsService = dataCollectionsService ?? throw new ArgumentNullException(nameof(dataCollectionsService));
            _likeService = likesService ?? throw new ArgumentNullException(nameof(likesService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Top100BestLikersModel> LoadTop100(long groupId, bool forceRecalc)
        {
            // const's
            var appToken = _configuration.GetValue<string>("VkAppToken");
            var currentDate = DateTime.Now;
            var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddSeconds(-1);

            var model = new Top100BestLikersModel
            {
                UsersInfo = null
            };

            var groupSetting = await _dbContext.GroupSettings.AsNoTracking().FirstOrDefaultAsync(f => f.GroupId == groupId);
            if (groupSetting != null)
            {
                if (groupSetting.ReverseGroup)
                {
                    groupId *= -1;
                }
                if (!string.IsNullOrEmpty(groupSetting.TopLikersHeaderMessage))
                {
                    model.TopUsersTitleText = groupSetting.TopLikersHeaderMessage;
                }
            }

            var calculationData = default(IDictionary<long, int>);
            var idIsPositive = groupId > 0;

            using (var locker = await KeyLocker<long>.LockAsync(groupId))
            {
                // get from DB
                calculationData = forceRecalc == false
                    ? await _dataCollectionsService.GetGroupLikesDataAsync(groupId, firstDayOfMonth, lastDayOfMonth)
                    : null;

                // Load from api & save if we don't have it
                if (forceRecalc || calculationData == null)
                {
                    calculationData = await _likeService.LoadAllLikesForCommunityPostsAsync(appToken, groupId, firstDayOfMonth, lastDayOfMonth, groupSetting);
                    await _dataCollectionsService.SaveGroupLikesDataAsync(groupId, firstDayOfMonth, lastDayOfMonth, calculationData, forceRecalc);
                }
            }

            // take just top 100 instead all
            var top100 = calculationData
                .OrderByDescending(x => x.Value)
                .Select(s => s.Key)
                .Take(100)
                .ToArray();

            model.Likes = calculationData;
            model.Top100 = top100;
            model.GroupUrlId = groupId.ToString();

            // if we have any top100 -> load theirs profile pictures
            if (top100.Length > 0)
            {
                var involvedUsers = await _likeService.GetUsersPhotoAndNames(appToken, top100);
                model.UsersInfo = involvedUsers;
            }

            return model;
        }
    }
}
