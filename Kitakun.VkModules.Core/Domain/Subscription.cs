namespace Kitakun.VkModules.Core.Domain
{
	using System;

	public class Subscription : IEntity
	{
		public int Id { get; set; }

		public int UserId { get; set; }

		public DateTime From { get; set; }

		public DateTime To { get; set; }
	}
}
