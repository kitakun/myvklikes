using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;

using Kitakun.VkModules.Services.Abstractions;
using Kitakun.VkModules.Web.WebModels;
using Kitakun.VkModules.Core.Extensions;

namespace Kitakun.VkModules.Web.Components
{
    public sealed class Top100BestLikersComponent : ViewComponent
    {
        private const string AppToken = "924ddd86924ddd86924ddd86df922afcec9924d924ddd86c9b8b1155b39d3337c8f8840";

        private readonly IGroupLikesService _likeService;
        private readonly IWebContext _webContext;
        private readonly IDataCollectionsService _dataCollectionsService;

        public Top100BestLikersComponent(
            IGroupLikesService likeService,
            IWebContext webContext,
            IDataCollectionsService dataCollectionsService)
        {
            _likeService = likeService ?? throw new ArgumentNullException(nameof(likeService));
            _webContext = webContext ?? throw new ArgumentNullException(nameof(webContext));
            _dataCollectionsService = dataCollectionsService ?? throw new ArgumentNullException(nameof(dataCollectionsService));
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // const's
            var currentDate = new DateTime(2017, 9, 5);
            var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var groupId = -29638784L;

            var calculationData = default(IDictionary<long, int>);
            using (var locker = await KeyLocker<long>.LockAsync(groupId))
            {
                // get from DB
                calculationData = await _dataCollectionsService.GetGroupLikesData(groupId, firstDayOfMonth, lastDayOfMonth);

                // Load from api & save if we don't have it
                if (calculationData == null)
                {
                    calculationData = await _likeService.LoadAllLikesForCommunityPostsAsync(AppToken, groupId, firstDayOfMonth, lastDayOfMonth);
                    await _dataCollectionsService.SaveGroupLikesData(groupId, firstDayOfMonth, lastDayOfMonth, calculationData);
                }
            }

            // take just top 100 instead all
            var top100 = calculationData
                .OrderByDescending(x => x.Value)
                .Select(s => s.Key)
                .Take(100)
                .ToArray();

            var model = new Top100BestLikersModel
            {
                Likes = calculationData,
                Top100 = top100,
                UsersInfo = null,
                IsAdmin = _webContext.IsAdmin
            };

            // if we have any top100 -> load theirs profile pictures
            if (top100.Length > 0)
            {
                var involvedUsers = await _likeService.GetUsersPhotoAndNames(AppToken, top100);
                model.UsersInfo = involvedUsers;
            }

            return View(nameof(Top100BestLikersComponent), model);
        }

        public static string GenerateLikerRow(Top100BestLikersModel model)
        {
            // Generate VK-API-JSON string for list control
            var sb = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                if (model.Top100.Length >= i + 1)
                {
                    bool hasNext = model.Top100.Length >= i + 2;
                    sb.AppendLine("{\\");
                    sb.AppendLine($"\"title\": loadedUsrs[{i}][\"last_name\"] + \" \" + loadedUsrs[{i}][\"first_name\"],\\");
                    sb.AppendLine($"\"title_url\": \"https://vk.com/id\" + loadedUsrs[{i}].id,\\");
                    sb.AppendLine($"\"icon_id\": \"id\" + loadedUsrs[{i}].id,\\");
                    sb.AppendLine($"\"descr\": \"Лайков: \" + top3likers[{i}]\\");
                    sb.AppendFormat("{1}{0}\\", hasNext ? "," : "", "}");
                }
            }
            return sb.ToString();
        }
    }
}
