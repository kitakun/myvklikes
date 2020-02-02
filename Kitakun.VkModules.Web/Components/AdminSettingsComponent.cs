namespace Kitakun.VkModules.Web.Components
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    using Kitakun.VkModules.Persistance;
    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Web.WebModels;

    /// <summary>
    /// "Глобальные" настройки для админа конечной группы
    /// </summary>
    public class AdminSettingsComponent : ViewComponent
    {
        private readonly IWebContext _webContext;
        private readonly IVkDbContext _dbContext;

        public AdminSettingsComponent(
            IWebContext webContext,
            IVkDbContext dbContext)
        {
            _webContext = webContext ?? throw new System.ArgumentNullException(nameof(webContext));
            _dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var groupSetting = await _dbContext
                .GroupSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.GroupId == _webContext.GroupId);

            return View(nameof(AdminSettingsComponent), new AdminSettingsComponentModel
            {
                AppToken = groupSetting.GroupAppToken,
                TopLikersHeaderMessage = groupSetting.TopLikersHeaderMessage,
                EnableAutoupdatingTop3 = groupSetting.BackgroundJobType == Core.Domain.BackgroundUpdaterType.Top3,
                EnableAutoupdatingTop5 = groupSetting.BackgroundJobType == Core.Domain.BackgroundUpdaterType.Top5
            });
        }
    }
}
