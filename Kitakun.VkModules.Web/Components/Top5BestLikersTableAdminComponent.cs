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

                IsAdmin = _webContext.IsAdmin || _webContext.IsUltraAdmin,
                TopUsersTitleText = groupSetting.TopLikersHeaderMessage
            });
        }

        // VK JNET Generator methods

        public static string GenerateCodeFromModel(Top100BestLikersAdminModel model, bool forApi = false)
        {
            var sb = new StringBuilder();

            sb.AppendLine("'\\");
            sb.AppendLine($"var widTitle = \"{model.TopUsersTitleText}\";\\");
            sb.AppendLine($"var top3Usrs = [{string.Join(',', model.Top100.Take(5))}];\\");
            sb.AppendLine($"var top3likers = [{GetTopRatesString(model)}];\\");
            sb.AppendLine("var loadedUsrs = API.users.get({ user_ids: top3Usrs, fields: \"sex\" });\\");
            sb.AppendLine("return {\\");
            sb.AppendLine("\"title\": widTitle,\\");
            sb.AppendLine("\"rows\": [\\");
            sb.AppendLine($"{GenerateLikerRowForApi(model, false)}\\");
            sb.AppendLine("]\\");
            sb.AppendLine("};'");

            return sb.ToString();
        }

        internal static string GetTopRatesString(Top100BestLikersAdminModel model)
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

        internal static string GenerateLikerRowForApi(Top100BestLikersAdminModel model, bool isApi = false)
        {
            // Generate VK-API-JSON string for list control
            var sb = new StringBuilder();
            var apiPref = isApi ? "\\" : string.Empty;
            for (int i = 0; i < 3; i++)
            {
                if (model.Top100.Length >= i + 1)
                {
                    bool hasNext = model.Top100.Length >= i + 2 && i != 2;
                    sb.AppendLine($"{{{apiPref}");
                    sb.AppendLine($"\"title\": loadedUsrs[{i}][\"last_name\"] + \" \" + loadedUsrs[{i}][\"first_name\"],{apiPref}");
                    sb.AppendLine($"\"title_url\": \"https://vk.com/id\" + loadedUsrs[{i}].id,{apiPref}");
                    sb.AppendLine($"\"icon_id\": \"id\" + loadedUsrs[{i}].id,{apiPref}");
                    sb.AppendLine($"\"descr\": \"Лайков: \" + top3likers[{i}]{apiPref}");
                    sb.AppendFormat("{1}{0}{3}{2}", (hasNext ? "," : ""), "}", Environment.NewLine, apiPref);
                }
            }
            return sb.ToString();
        }
    }
}
