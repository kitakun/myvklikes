namespace Kitakun.VkModules.Core.Domain
{
	using System;

	public class DateCollection : IEntity
	{
		public int Id { get; set; }

		public DateTime From { get; set; }

		public DateTime To { get; set; }

		public CollectionDataType Type { get; set; }

		public string JsonValue { get; set; }
	}
}
