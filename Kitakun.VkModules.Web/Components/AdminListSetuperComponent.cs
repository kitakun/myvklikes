using Microsoft.AspNetCore.Mvc;

namespace Kitakun.VkModules.Web.Components
{
    /// <summary>
    /// Вывести одного пользователя
    /// </summary>
	public class AdminListSetuperComponent : ViewComponent
	{
		public IViewComponentResult Invoke() => View(nameof(AdminListSetuperComponent));
	}
}
