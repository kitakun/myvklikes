using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kitakun.VkModules.Services.Abstractions
{
    public interface IDataCollectionsService
    {
        Task<IDictionary<long, int>> GetGroupLikesData(long groupId, DateTime from, DateTime to);
        Task SaveGroupLikesData(long groupId, DateTime from, DateTime to, IDictionary<long, int> data);
    }
}
