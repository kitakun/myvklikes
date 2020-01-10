namespace Kitakun.VkModules.Web.Components
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Mvc;

    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Web.WebModels;
    using Kitakun.VkModules.Core.Extensions;
    using Kitakun.VkModules.Persistance;
    using Microsoft.EntityFrameworkCore;

    public sealed class Top100BestLikersComponent : ViewComponent
    {
        private const string AppToken = "924ddd86924ddd86924ddd86df922afcec9924d924ddd86c9b8b1155b39d3337c8f8840";

        private readonly IGroupLikesService _likeService;
        private readonly IWebContext _webContext;
        private readonly IDataCollectionsService _dataCollectionsService;
        private readonly IVkDbContext _dbContext;

        public Top100BestLikersComponent(
            IGroupLikesService likeService,
            IWebContext webContext,
            IDataCollectionsService dataCollectionsService,
            IVkDbContext dbContext)
        {
            _likeService = likeService ?? throw new ArgumentNullException(nameof(likeService));
            _webContext = webContext ?? throw new ArgumentNullException(nameof(webContext));
            _dataCollectionsService = dataCollectionsService ?? throw new ArgumentNullException(nameof(dataCollectionsService));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // const's
            var currentDate = DateTime.Now;
            var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var groupId = _webContext.GroupId;

            var model = new Top100BestLikersModel
            {
                UsersInfo = null,
                IsAdmin = _webContext.IsUltraAdmin || _webContext.IsAdmin
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

            var recalc = HttpContext.Request.Query.TryGetValue("recalc", out var _);

            var calculationData = default(IDictionary<long, int>);
            var idIsPositive = groupId > 0;

            using (var locker = await KeyLocker<long>.LockAsync(groupId))
            {
                // get from DB
                calculationData = recalc == false
                    ? await _dataCollectionsService.GetGroupLikesDataAsync(groupId, firstDayOfMonth, lastDayOfMonth)
                    : null;

                // Load from api & save if we don't have it
                if (recalc || calculationData == null)
                {
                    calculationData = await _likeService.LoadAllLikesForCommunityPostsAsync(AppToken, groupId, firstDayOfMonth, lastDayOfMonth);
                    await _dataCollectionsService.SaveGroupLikesDataAsync(groupId, firstDayOfMonth, lastDayOfMonth, calculationData, recalc);
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

            // if we have any top100 -> load theirs profile pictures
            if (top100.Length > 0)
            {
                var involvedUsers = await _likeService.GetUsersPhotoAndNames(AppToken, top100);
                model.UsersInfo = involvedUsers;
            }

            return View(nameof(Top100BestLikersComponent), model);
        }

        // VK JNET Generator methods

        public static string GenerateCodeFromModel(Top100BestLikersModel model)
        {
            var sb = new StringBuilder();

            sb.AppendLine("'\\");
            sb.AppendLine($"var widTitle = \"{model.TopUsersTitleText}\";\\");
            sb.AppendLine($"var top3Usrs = [{string.Join(',', model.Top100.Take(3))}];\\");
            sb.AppendLine($"var top3likers = [{GetTopRatesString(model)}];\\");
            sb.AppendLine("var loadedUsrs = API.users.get({ user_ids: top3Usrs, fields: \"sex\" });\\");
            sb.AppendLine("return {\\");
            sb.AppendLine("\"title\": widTitle,\\");
            sb.AppendLine("\"rows\": [\\");
            sb.AppendLine($"{GenerateLikerRow(model)}\\");
            sb.AppendLine("]\\");
            sb.AppendLine("};'");

            return sb.ToString();
        }

        public static string GenerateCodeFromModelForApi(Top100BestLikersModel model)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"var widTitle = \"{model.TopUsersTitleText}\";");
            sb.AppendLine($"var top3Usrs = [{string.Join(',', model.Top100.Take(3))}];");
            sb.AppendLine($"var top3likers = [{GetTopRatesString(model)}];");
            sb.AppendLine("var loadedUsrs = API.users.get({ user_ids: top3Usrs, fields: \"sex\" });");
            sb.AppendLine("return {");
            sb.AppendLine("\"title\": widTitle,");
            sb.AppendLine("\"rows\": [");
            sb.AppendLine($"{GenerateLikerRowForApi(model)}");
            sb.AppendLine("]");
            sb.AppendLine("};");

            return sb.ToString();
        }

        internal static string GenerateLikerRow(Top100BestLikersModel model)
        {
            // Generate VK-API-JSON string for list control
            var sb = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                if (model.Top100.Length >= i + 1)
                {
                    bool hasNext = model.Top100.Length >= i + 2 && i != 2;
                    sb.AppendLine("{\\");
                    sb.AppendLine($"\"title\": loadedUsrs[{i}][\"last_name\"] + \" \" + loadedUsrs[{i}][\"first_name\"],\\");
                    sb.AppendLine($"\"title_url\": \"https://vk.com/id\" + loadedUsrs[{i}].id,\\");
                    sb.AppendLine($"\"icon_id\": \"id\" + loadedUsrs[{i}].id,\\");
                    sb.AppendLine($"\"descr\": \"Лайков: \" + top3likers[{i}]\\");
                    sb.AppendFormat("{1}{0}\\{2}", (hasNext ? "," : ""), "}", Environment.NewLine);
                }
            }
            return sb.ToString();
        }

        internal static string GenerateLikerRowForApi(Top100BestLikersModel model)
        {
            // Generate VK-API-JSON string for list control
            var sb = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                if (model.Top100.Length >= i + 1)
                {
                    bool hasNext = model.Top100.Length >= i + 2 && i != 2;
                    sb.AppendLine("{");
                    sb.AppendLine($"\"title\": loadedUsrs[{i}][\"last_name\"] + \" \" + loadedUsrs[{i}][\"first_name\"],");
                    sb.AppendLine($"\"title_url\": \"https://vk.com/id\" + loadedUsrs[{i}].id,");
                    sb.AppendLine($"\"icon_id\": \"id\" + loadedUsrs[{i}].id,");
                    sb.AppendLine($"\"descr\": \"Лайков: \" + top3likers[{i}]");
                    sb.AppendFormat("{1}{0}{2}", (hasNext ? "," : ""), "}", Environment.NewLine);
                }
            }
            return sb.ToString();
        }

        internal static string GetTopRatesString(Top100BestLikersModel model)
        {
            var sb = new StringBuilder();
            var first = true;
            for (var i = 0; i < 3 && i < model.Top100.Length; i++)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(",");
                }
                sb.Append($"{model.Likes[model.Top100[i]]}");
            }
            return sb.ToString();
        }
    }
}
