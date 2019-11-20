using Kitakun.VkModules.Core.Domain;

namespace Kitakun.VkModules.Web.Areas.UltraAdmin.Models
{
	public class HomeViewModel
	{
		/// <summary>
		/// Filter query (long)
		/// </summary>
		public string SearchString { get; set; }
		/// <summary>
		/// Founded by filter or initial entities
		/// </summary>
		public Subscription[] Subscriptions { get; set; }

		public string ExceptionMessage { get; set; }
		public string WarningMessage { get; set; }
	}
}
