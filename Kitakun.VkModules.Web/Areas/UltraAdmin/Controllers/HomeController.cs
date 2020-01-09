namespace Kitakun.VkModules.Web.Areas.UltraAdmin.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.EntityFrameworkCore;

    using Kitakun.VkModules.Web.Infrastructure;
    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Web.Areas.UltraAdmin.Models;
    using Kitakun.VkModules.Persistance;

    [AllowAnonymous]
    [Area(AreaNames.UltraAdminAreaName)]
    [EnableCors(WebConstants.AllCorsName)]
    public class HomeController : Controller
    {
        private readonly IWebContext _webContext;
        private readonly IVkDbContext _dbContext;

        public HomeController(
            IWebContext webContext,
            IVkDbContext dbContext)
        {
            _webContext = webContext ?? throw new ArgumentNullException(nameof(webContext));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!_webContext.IsUltraAdmin)
            {
                return NotFound();
            }

            var model = new HomeViewModel
            {
                SearchString = string.Empty
            };
            try
            {
                const int takeOnly = 5;

                model.Subscriptions = await _dbContext
                    .Subscriptions
                    .AsNoTracking()
                    .OrderBy(x => x.From)
                    .Take(takeOnly)
                    .ToArrayAsync();

                model.WarningMessage = $"Показано {takeOnly} элементов по дате начала";
            }
            catch (Exception es)
            {
                model.ExceptionMessage = es.Message;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromForm] string searchString)
        {
            var model = new HomeViewModel
            {
                SearchString = searchString
            };

            try
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    model.Subscriptions = await _dbContext
                        .Subscriptions
                        .AsNoTracking()
                        .Where(w => searchString.Contains(w.UserId.ToString()) || searchString.Contains(w.GroupId.ToString()))
                        .ToArrayAsync();
                }
                else
                {
                    const int takeOnly = 5;

                    model.Subscriptions = await _dbContext
                        .Subscriptions
                        .AsNoTracking()
                        .OrderBy(x => x.From)
                        .Take(takeOnly)
                        .ToArrayAsync();

                    model.WarningMessage = $"Показано {takeOnly} элементов по дате начала";
                }
            }
            catch (Exception es)
            {
                model.ExceptionMessage = es.Message;
            }

            return View(nameof(Index), model);
        }
    }
}
