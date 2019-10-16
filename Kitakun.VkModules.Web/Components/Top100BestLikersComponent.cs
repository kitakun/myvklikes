using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Kitakun.VkModules.Services.Abstractions;
using Kitakun.VkModules.Web.WebModels;
using System.Text;

namespace Kitakun.VkModules.Web.Components
{
	public class Top100BestLikersComponent : ViewComponent
	{
		private const string AppToken = "924ddd86924ddd86924ddd86df922afcec9924d924ddd86c9b8b1155b39d3337c8f8840";

		private readonly IGroupLikesService _likeService;
		private readonly IWebContext _webContext;

		private static IDictionary<long, int> likesInfo = null;

		public Top100BestLikersComponent(
			IGroupLikesService likeService,
			IWebContext webContext)
		{
			_likeService = likeService ?? throw new ArgumentNullException(nameof(likeService));
			_webContext = webContext ?? throw new ArgumentNullException(nameof(webContext));
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var currentDate = new DateTime(2017, 9, 5);
			var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
			var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

			var groupId = -29638784L;
			if (likesInfo == null)
			{
				likesInfo = await _likeService.LoadAllLikesForCommunityPostsAsync(AppToken, groupId, firstDayOfMonth, lastDayOfMonth);
			}
			var top100 = likesInfo.OrderByDescending(x => x.Value).Select(s => s.Key).Take(100).ToArray();

			var model = new Top100BestLikersModel
			{
				Likes = likesInfo,
				Top100 = top100,
				UsersInfo = null,
				IsAdmin = _webContext.IsAdmin
			};

			if (top100.Length > 0)
			{
				var involvedUsers = await _likeService.GetUsersPhotoAndNames(AppToken, top100);
				model.UsersInfo = involvedUsers;
			}

			return View(nameof(Top100BestLikersComponent), model);
		}

		public static string GenerateLikerRow(Top100BestLikersModel model)
		{
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
