namespace Kitakun.VkModules.Web.Areas.UltraAdmin.Controllers
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.EntityFrameworkCore;

    using Kitakun.VkModules.Web.Infrastructure;
    using Kitakun.VkModules.Persistance;
    using Kitakun.VkModules.Core.Domain;

    [AllowAnonymous]
    [Area(AreaNames.UltraAdminAreaName)]
    [EnableCors(WebConstants.AllCorsName)]
    public class GroupSettingsController : Controller
    {
        private readonly IVkDbContext _dbContext;

        public GroupSettingsController(IVkDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [HttpGet]
        public IActionResult Create() => View("Index", new GroupSettings());

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] long? groupId)
        {
            var expectedEntity = await _dbContext.GroupSettings.FirstOrDefaultAsync(w => w.GroupId == groupId.Value);
            if (expectedEntity == null)
                return NotFound();

            return View("Index", expectedEntity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] GroupSettings model)
        {
            await _dbContext.GroupSettings.AddAsync(model);

            await _dbContext.SaveChangesAsync();

            ViewBag.Message = "Успешно создано";

            return View("Index", model);
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromForm] GroupSettings model)
        {
            var existingEntity = await _dbContext.GroupSettings.FindAsync(model.Id);
            if (existingEntity == null)
                return NotFound();

            existingEntity.ReverseGroup = model.ReverseGroup;
            existingEntity.GroupAppToken = model.GroupAppToken;
            existingEntity.TopLikersHeaderMessage = model.TopLikersHeaderMessage;

            await _dbContext.SaveChangesAsync();

            ViewBag.Message = "Успешно обнавлено";

            return View("Index", existingEntity);
        }
    }
}
