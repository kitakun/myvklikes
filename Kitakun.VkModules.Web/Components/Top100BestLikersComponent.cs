namespace Kitakun.VkModules.Web.Components
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Core.Models;

    /// <summary>
    /// Топ 3 виджет
    /// </summary>
    public sealed class Top100BestLikersComponent : ViewComponent
    {
        private readonly IWebContext _webContext;
        private readonly ITop100Service _top100Service;

        public Top100BestLikersComponent(
            IWebContext webContext,
            ITop100Service top100Service)
        {
            _webContext = webContext ?? throw new ArgumentNullException(nameof(webContext));
            _top100Service = top100Service ?? throw new ArgumentNullException(nameof(top100Service));
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var recalc = HttpContext.Request.Query.TryGetValue("recalc", out var _);

            var model = await _top100Service.LoadTop100(_webContext.GroupId, recalc);

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
