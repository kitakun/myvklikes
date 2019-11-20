namespace Kitakun.VkModules.Web.Areas.UltraAdmin.Controllers
{
	using System;
	using System.Threading.Tasks;

	using Microsoft.AspNetCore.Cors;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Authorization;

	using Kitakun.VkModules.Web.Infrastructure;
	using Kitakun.VkModules.Web.Areas.UltraAdmin.Models;
	using Kitakun.VkModules.Persistance;
	using Kitakun.VkModules.Core.Domain;

	[AllowAnonymous]
	[Area(AreaNames.UltraAdminAreaName)]
	[EnableCors(WebConstants.AllCorsName)]
	public class SubscriptionsController : Controller
	{
		private readonly IVkDbContext _dbContext;

		public SubscriptionsController(IVkDbContext dbContext)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		public IActionResult Index() => RedirectToActionPermanent("Index", "Home");

		[HttpGet]
		public IActionResult Create() =>
			View(new CreateSubscriptionModel
			{
				From = DateTime.Now.Date,
				To = DateTime.Now.Date.AddMonths(1)
			});

		[HttpPost]
		public async Task<IActionResult> Create([FromForm] CreateSubscriptionModel model)
		{
			var newSubscription = new Subscription
			{
				From = model.From,
				To = model.To,
				UserId = model.UserId,
				GroupId = model.GroupId
			};
			await _dbContext.Subscriptions.AddAsync(newSubscription);
			await _dbContext.SaveChangesAsync();

			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> Edit([FromQuery] int? id)
		{
			var subscription = await _dbContext.Subscriptions.FindAsync(id.Value);

			return View("Create", new CreateSubscriptionModel
			{
				Id = subscription.Id,
				From = subscription.From,
				To = subscription.To,
				GroupId = subscription.GroupId,
				UserId = subscription.UserId
			});
		}

		[HttpPost]
		public async Task<IActionResult> Edit([FromForm] CreateSubscriptionModel model)
		{
			var subscription = await _dbContext.Subscriptions.FindAsync(model.Id);

			subscription.From = model.From;
			subscription.To = model.To;
			subscription.UserId = model.UserId;
			subscription.GroupId = model.GroupId;

			await _dbContext.SaveChangesAsync();

			return View("Create", model);
		}
	}
}
