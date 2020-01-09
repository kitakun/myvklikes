namespace Kitakun.VkModules.Web.Controllers
{
#if DEBUG
    using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Cors;
	using Microsoft.AspNetCore.Mvc;

	using Kitakun.VkModules.Web.Infrastructure;
	using Kitakun.VkModules.Services.Abstractions;

	//[Authorize]
	//[ApiController]
	[EnableCors(WebConstants.AllCorsName)]
	public class LikesController : Controller
	{
		private const string AppToken = "924ddd86924ddd86924ddd86df922afcec9924d924ddd86c9b8b1155b39d3337c8f8840";
		private readonly IGroupLikesService _groupLikeService;

		public LikesController(IGroupLikesService groupLikeService)
		{
			_groupLikeService = groupLikeService ?? throw new ArgumentNullException(nameof(groupLikeService));
		}

		[HttpGet]
		public Task<IDictionary<long, int>> Get()
		{
			var currentDate = new DateTime(2017, 9, 5);
			var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
			var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

			var groupId = -29638784L;

			return _groupLikeService.LoadAllLikesForCommunityPostsAsync(AppToken, groupId, firstDayOfMonth, lastDayOfMonth);
		}
	}
#endif
}
