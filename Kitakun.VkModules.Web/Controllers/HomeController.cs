namespace Kitakun.VkModules.Web.Controllers
{
	using Microsoft.AspNetCore.Cors;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Authorization;

	using Kitakun.VkModules.Web.Infrastructure;
	using Kitakun.VkModules.Web.WebModels;
	using Kitakun.VkModules.Services.Abstractions;

	[AllowAnonymous]
	[EnableCors(WebConstants.AllCorsName)]
	public class HomeController : Controller
	{
		private readonly IWebContext _webContext;

		public HomeController(IWebContext webContext)
		{
			_webContext = webContext ?? throw new System.ArgumentNullException(nameof(webContext));
		}

		[HttpGet]
		public IActionResult Index() => View(new HomeModel
		{
			IsAdmin = _webContext.IsUltraAdmin || _webContext.IsAdmin
		});
	}
}
