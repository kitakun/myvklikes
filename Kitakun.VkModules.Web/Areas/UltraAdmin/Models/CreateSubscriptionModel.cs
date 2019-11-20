using System;

namespace Kitakun.VkModules.Web.Areas.UltraAdmin.Models
{
	public class CreateSubscriptionModel
	{
		public int? Id { get; set; }

		public long UserId { get; set; }

		public long? GroupId { get; set; }

		public DateTime From { get; set; }

		public DateTime To { get; set; }
	}
}
