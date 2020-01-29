namespace Kitakun.VkModules.Web.Controllers
{
    using System.Threading.Tasks;

    using Hangfire;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    using Kitakun.VkModules.Web.Infrastructure;
    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Web.WebModels;
    using Kitakun.VkModules.Persistance;
    using Kitakun.VkModules.Web.Extensions;

    [AllowAnonymous]
    [EnableCors(WebConstants.AllCorsName)]
    public sealed class AdminController : Controller
    {
        private readonly IWebContext _webContext;
        private readonly ISessionSecretGenerator _sessionSecret;
        private readonly IVkDbContext _dbContext;
        private readonly BackgroundUpdater _bgUpdater;

        public AdminController(
            IWebContext webContext,
            ISessionSecretGenerator sessionSecret,
            IVkDbContext dbContext,
            BackgroundUpdater bgUpdater)
        {
            _webContext = webContext ?? throw new System.ArgumentNullException(nameof(webContext));
            _sessionSecret = sessionSecret ?? throw new System.ArgumentNullException(nameof(sessionSecret));
            _dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));
            _bgUpdater = bgUpdater ?? throw new System.ArgumentNullException(nameof(bgUpdater));
        }

        [HttpGet]
        public IActionResult GetSecret()
            => _webContext.IsAdmin
                ? Ok(_sessionSecret.GetSessionSecret()) as IActionResult
                : NotFound();

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> UpdateSettings([FromForm] AdminSettingsComponentModel model)
        {
            var groupSetting = await _dbContext
                .GroupSettings
                .FirstOrDefaultAsync(f => f.GroupId == _webContext.GroupId);

            if (groupSetting != null)
            {
                groupSetting.GroupAppToken = model.AppToken;
                groupSetting.TopLikersHeaderMessage = model.TopLikersHeaderMessage;

                var skipEnablingBackgroundJob = false;
                if (!string.IsNullOrEmpty(groupSetting.RecuringBackgroundJobId))
                {
                    // skip same works re-enabling
                    switch (groupSetting.BackgroundJobType)
                    {
                        case Core.Domain.BackgroundUpdaterType.Top3:
                            if (!model.EnableAutoupdatingTop3)
                            {
                                skipEnablingBackgroundJob = true;
                            }
                            break;
                    }

                    // Remove previous work
                    RecurringJob.RemoveIfExists(groupSetting.RecuringBackgroundJobId);
                    groupSetting.RecuringBackgroundJobId = null;
                }

                if (!skipEnablingBackgroundJob && groupSetting.GroupId.HasValue)
                {
                    var groupId = groupSetting.GroupId.Value;

                    if (model.EnableAutoupdatingTop3)
                    {
                        groupSetting.BackgroundJobType = Core.Domain.BackgroundUpdaterType.Top3;
                    }

                    groupSetting.RecuringBackgroundJobId = $"RecurringJob={groupId}";

                    RecurringJob.AddOrUpdate(groupSetting.RecuringBackgroundJobId, () => _bgUpdater.Run(groupId), WebConstants.Every15minterCron);
                }

                if(model.EnableAutoupdatingTop3 == false)
                {
                    groupSetting.BackgroundJobType = Core.Domain.BackgroundUpdaterType.Undefined;
                }

                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(
                nameof(HomeController.Index),
                nameof(HomeController).Replace("Controller", ""),
                Request.ToQuery());
        }
    }
}
