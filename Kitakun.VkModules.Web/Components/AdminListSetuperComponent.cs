using Microsoft.AspNetCore.Mvc;

namespace Kitakun.VkModules.Web.Components
{
	public class AdminListSetuperComponent : ViewComponent
	{
		public IViewComponentResult Invoke() => View(nameof(AdminListSetuperComponent));
	}
}
