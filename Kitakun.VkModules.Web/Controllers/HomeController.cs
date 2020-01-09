namespace Kitakun.VkModules.Web.Controllers
{
    using System.Threading.Tasks;

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
        private readonly ISubscriptionsService _subService;

        public HomeController(
            IWebContext webContext,
            ISubscriptionsService subService)
        {
            _webContext = webContext ?? throw new System.ArgumentNullException(nameof(webContext));
            _subService = subService ?? throw new System.ArgumentNullException(nameof(subService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var groupId = _webContext.GroupId;

            var hasSub = groupId != 0
                ? await _subService.GroupHasSubscription(groupId)
                : false;

            return View(
                new HomeModel
                {
                    HasSubscription = hasSub,
                    IsAdmin = _webContext.IsUltraAdmin || _webContext.IsAdmin
                }
            );
        }
    }
}
