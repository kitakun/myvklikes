namespace Kitakun.VkModules.Core.Domain
{
	using System;

	public class DataCollection : IEntity
	{
		public int Id { get; set; }

        /// <summary>
        /// Calculation external owner (groupId)
        /// </summary>
        public long OwnerExternalId { get; set; }

        /// <summary>
        /// Data calculated for period FROM
        /// </summary>
		public DateTime From { get; set; }
        /// <summary>
        /// Data calculated for period TO
        /// </summary>
		public DateTime To { get; set; }

        /// <summary>
        /// When was calculation performed
        /// </summary>
        public DateTime CalculationStartedAt { get; set; }

        /// <summary>
        /// Calculation type
        /// </summary>
		public CollectionDataType Type { get; set; }

        /// <summary>
        /// Our service Json data
        /// </summary>
		public string JsonValue { get; set; }
	}
}
