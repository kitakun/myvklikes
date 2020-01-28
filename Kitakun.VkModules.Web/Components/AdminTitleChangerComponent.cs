using Microsoft.AspNetCore.Mvc;

namespace Kitakun.VkModules.Web.Components
{
    /// <summary>
    /// Табличка с текстом
    /// </summary>
	public class AdminTitleChangerComponent : ViewComponent
	{
		public IViewComponentResult Invoke() => View(nameof(AdminTitleChangerComponent));
	}
}
