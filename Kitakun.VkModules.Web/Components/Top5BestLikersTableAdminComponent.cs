namespace Kitakun.VkModules.Web.Components
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Web.WebModels;
    using Kitakun.VkModules.Persistance;

    public class Top5BestLikersTableAdminComponent : ViewComponent
    {
        private readonly IWebContext _webContext;
        private readonly IVkDbContext _dbContext;
        private readonly ITop100Service _top100Service;

        public Top5BestLikersTableAdminComponent(
            IWebContext webContext,
            IVkDbContext dbContext,
            ITop100Service top100Service)
        {
            _webContext = webContext ?? throw new ArgumentNullException(nameof(webContext));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _top100Service = top100Service ?? throw new ArgumentNullException(nameof(top100Service));
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var groupId = _webContext.GroupId;

            var model = await _top100Service.LoadTop100(groupId, false);
            var groupSetting = await _dbContext.GroupSettings.AsNoTracking().FirstOrDefaultAsync(f => f.GroupId == groupId);

            return View(nameof(Top5BestLikersTableAdminComponent), new Top100BestLikersAdminModel
            {
                Likes = model.Likes,
                Top100 = model.Top100,
                UsersInfo = model.UsersInfo,
                GroupUrlId = model.GroupUrlId,

                IsAdmin = _webContext.IsAdmin || _webContext.IsUltraAdmin,
                TopUsersTitleText = groupSetting.TopLikersHeaderMessage
            });
        }

        // VK JNET Generator methods

        public static string GenerateCodeFromModel(Core.Models.Top100BestLikersModel model, bool isApi = false)
        {
            var sb = new StringBuilder();
            var apiPref = isApi ? string.Empty : "\\";
            if (!isApi)
            {
                sb.AppendLine($"'{apiPref}");
            }
            sb.AppendLine($"var widTitle = \"{model.TopUsersTitleText}\";{apiPref}");
            sb.AppendLine($"var top5Usrs = [{string.Join(',', model.Top100.Take(5))}];{apiPref}");
            sb.AppendLine($"var top5likers = [{GetTopRatesString(model)}];{apiPref}");
            sb.AppendLine($"var loadedUsrs = API.users.get({{ user_ids: top5Usrs, fields: \"sex\" }});{apiPref}");
            sb.AppendLine($"return {{{apiPref}");
            sb.AppendLine($"\"title\": widTitle,{apiPref}");
            sb.Append($"{AppendToAppUrlAtBottom(model, isApi)}");
            //head
            sb.AppendLine($"\"head\": [{apiPref}");
            sb.AppendLine($"{{\"text\": \"Пользователь\"}},{apiPref}");
            sb.AppendLine($"{{\"text\": \"Баллы\", \"align\": \"center\" }},{apiPref}");
            sb.AppendLine($"],{apiPref}");

            sb.AppendLine($"\"body\": [{apiPref}");
            sb.AppendLine($"{GenerateLikerRowForApi(model, isApi)}{apiPref}");
            sb.AppendLine($"]{apiPref}");
            sb.AppendLine($"}};{apiPref}");
            if (!isApi)
            {
                sb.AppendLine("'");
            }

            return sb.ToString();
        }

        private static string AppendToAppUrlAtBottom(Core.Models.Top100BestLikersModel model, bool isApi = false)
        {
            var sb = new StringBuilder();
            var apiPref = isApi ? string.Empty : "\\";

            sb.AppendLine($"\"more\": \"Посмотреть всю таблицу\",{apiPref}");
            sb.AppendLine($"\"more_url\": \"https://vk.com/app6758762_{model.GroupUrlId}\",{apiPref}");

            return sb.ToString();
        }

        private static string GetTopRatesString(Core.Models.Top100BestLikersModel model)
        {
            var sb = new StringBuilder();
            var first = true;
            for (var i = 0; i < 5 && i < model.Top100.Length; i++)
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

        private static string GenerateLikerRowForApi(Core.Models.Top100BestLikersModel model, bool isApi = false)
        {
            // Generate VK-API-JSON string for list control
            var sb = new StringBuilder();
            var apiPref = isApi ? string.Empty : "\\";
            for (int i = 0; i < 5; i++)
            {
                if (model.Top100.Length >= i + 1)
                {
                    bool hasNext = model.Top100.Length >= i + 1 && i != 5;
                    sb.AppendLine($"[{{{apiPref}");
                    sb.AppendLine($"\"text\": loadedUsrs[{i}][\"last_name\"] + \" \" + loadedUsrs[{i}][\"first_name\"],{apiPref}");
                    sb.AppendLine($"\"url\": \"https://vk.com/id\" + loadedUsrs[{i}].id,{apiPref}");
                    sb.AppendLine($"\"icon_id\": \"id\" + loadedUsrs[{i}].id{apiPref}");
                    sb.AppendLine($"}},{apiPref}");

                    sb.AppendLine($"{{{apiPref}");
                    sb.AppendLine($"\"text\": top5likers[{i}]{apiPref}");
                    sb.AppendLine($"}}]{apiPref}");

                    sb.AppendFormat($"{(hasNext ? "," : string.Empty)}");
                }
            }
            return sb.ToString();
        }
    }
}
