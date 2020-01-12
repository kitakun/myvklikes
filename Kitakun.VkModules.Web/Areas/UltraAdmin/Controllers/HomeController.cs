namespace Kitakun.VkModules.Web.Areas.UltraAdmin.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Hangfire;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.EntityFrameworkCore;

    using Kitakun.VkModules.Web.Infrastructure;
    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Web.Areas.UltraAdmin.Models;
    using Kitakun.VkModules.Persistance;
    using Kitakun.VkModules.Web.Controllers;

    [AllowAnonymous]
    [Area(AreaNames.UltraAdminAreaName)]
    [EnableCors(WebConstants.AllCorsName)]
    public class HomeController : Controller
    {
        const int takeOnly = 5;

        private readonly IWebContext _webContext;
        private readonly VkDbContext _dbContext;
        private readonly BackgroundUpdater _bgUpdater;

        public HomeController(
            IWebContext webContext,
            VkDbContext dbContext,
            BackgroundUpdater bgUpdater)
        {
            _webContext = webContext ?? throw new ArgumentNullException(nameof(webContext));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this._bgUpdater = bgUpdater ?? throw new ArgumentNullException(nameof(bgUpdater));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
#if RELEASE
            if (!_webContext.IsUltraAdmin)
            {
                return NotFound();
            }
#endif

            var model = new HomeViewModel
            {
                SearchString = string.Empty
            };
            try
            {
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
        public async Task<IActionResult> SearchSubs([FromForm] string searchString)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchGroupSetting([FromForm] string searchGroupSettings)
        {
            if (long.TryParse(searchGroupSettings, out var parsedGroupId))
            {
                var hasSetts = await _dbContext.GroupSettings.AnyAsync(a => a.GroupId == parsedGroupId);
                if (hasSetts)
                {
                    return RedirectToAction(
                        nameof(GroupSettingsController.Index),
                        nameof(GroupSettingsController).Replace("Controller", ""),
                        new
                        {
                            groupId = parsedGroupId
                        });
                }
                else
                {
                    return RedirectToAction(
                        nameof(GroupSettingsController.Create),
                        nameof(GroupSettingsController).Replace("Controller", ""));
                }
            }
            else
            {
                var model = new HomeViewModel
                {
                    SearchString = string.Empty,
                    WarningMessage = $"Can't parse group Id for '{searchGroupSettings}'",
                    Subscriptions = await _dbContext
                        .Subscriptions
                        .AsNoTracking()
                        .OrderBy(x => x.From)
                        .Take(takeOnly)
                        .ToArrayAsync()
                };
                return View(nameof(Index), model);
            }
        }

        [HttpPost]
        public Task UpdateDatabase() => _dbContext.Database.MigrateAsync();

        public async Task<IActionResult> RunBackgroundUpdate()
        {
            var groupIdsWithToken = await _dbContext
                .GroupSettings
                .Where(w => w.GroupAppToken.Length > 0 && w.GroupId.HasValue)
                .Select(s => s.GroupId.Value)
                .ToArrayAsync();

            for (var i = 0; i < groupIdsWithToken.Length; i++)
            {
                const string every15minterCron = "*/15 * * * *";
                RecurringJob.AddOrUpdate(() => _bgUpdater.Run(groupIdsWithToken[i]), every15minterCron);
            }

            return Ok("done");
        }
    }
}
