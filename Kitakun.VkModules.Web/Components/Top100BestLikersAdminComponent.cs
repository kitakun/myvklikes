namespace Kitakun.VkModules.Web.Components
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Web.WebModels;
    using Kitakun.VkModules.Persistance;

    /// <summary>
    /// Топ 3 виджет - админка
    /// </summary>
    public class Top100BestLikersAdminComponent : ViewComponent
    {
        private readonly IWebContext _webContext;
        private readonly IVkDbContext _dbContext;
        private readonly ITop100Service _top100Service;

        public Top100BestLikersAdminComponent(
            IWebContext webContext,
            IVkDbContext dbContext,
            ITop100Service top100Service)
        {
            _webContext = webContext ?? throw new System.ArgumentNullException(nameof(webContext));
            _dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));
            _top100Service = top100Service ?? throw new System.ArgumentNullException(nameof(top100Service));
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var groupId = _webContext.GroupId;

            var model = await _top100Service.LoadTop100(groupId, false);
            var groupSetting = await _dbContext.GroupSettings.AsNoTracking().FirstOrDefaultAsync(f => f.GroupId == groupId);

            return View(nameof(Top100BestLikersAdminComponent), new Top100BestLikersAdminModel
            {
                Likes = model.Likes,
                Top100 = model.Top100,
                UsersInfo = model.UsersInfo,
                GroupUrlId = model.GroupUrlId,

                IsAdmin = _webContext.IsAdmin || _webContext.IsUltraAdmin,
                TopUsersTitleText = groupSetting.TopLikersHeaderMessage
            });
        }
    }
}
