using Kitakun.VkModules.Services.Abstractions;

namespace Kitakun.VkModules.Web.Services
{
	public class WebContext : IWebContext
	{
		public bool IsAdmin => false;
	}
}
