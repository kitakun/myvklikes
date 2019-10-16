using Microsoft.AspNetCore.Mvc;

namespace Kitakun.VkModules.Web.Components
{
	public class AdminTitleChangerComponent : ViewComponent
	{
		public IViewComponentResult Invoke() => View(nameof(AdminTitleChangerComponent));
	}
}
