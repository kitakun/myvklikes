using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

using Kitakun.VkModules.Core.Domain;
using Kitakun.VkModules.Persistance;
using Kitakun.VkModules.Services.Abstractions;

namespace Kitakun.VkModules.Services.PersistanceServices
{
	public sealed class DataCollectionsService : IDataCollectionsService
	{
		private readonly IVkDbContext _dbContext;

		public DataCollectionsService(IVkDbContext dbContext)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		public async Task<IDictionary<long, int>> GetGroupLikesData(long groupId, DateTime from, DateTime to)
		{
			string foundedData = await _dbContext
				.DataCollections
				.AsNoTracking()
				.Where(w => w.Type == CollectionDataType.LikesOnPostsForTimeRange
					&& w.From == from
					&& w.To == to
					&& w.OwnerExternalId == groupId)
				.OrderByDescending(x => x.CalculationStartedAt)
				.Select(s => s.JsonValue)
				.FirstOrDefaultAsync();

			return !string.IsNullOrEmpty(foundedData)
					? JsonConvert.DeserializeObject<Dictionary<long, int>>(foundedData)
					: null;
		}

		public async Task SaveGroupLikesData(long groupId, DateTime from, DateTime to, IDictionary<long, int> data)
		{
			var newDataCollection = new DataCollection
			{
				From = from,
				To = to,
				CalculationStartedAt = DateTime.UtcNow,
				OwnerExternalId = groupId,
				Type = CollectionDataType.LikesOnPostsForTimeRange,
				JsonValue = JsonConvert.SerializeObject(data)
			};

			await _dbContext.DataCollections.AddAsync(newDataCollection);

			await _dbContext.SaveChangesAsync();
		}
	}
}
