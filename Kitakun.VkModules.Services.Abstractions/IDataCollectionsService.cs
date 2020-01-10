namespace Kitakun.VkModules.Services.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDataCollectionsService
    {
        Task<IDictionary<long, int>> GetGroupLikesDataAsync(long groupId, DateTime from, DateTime to);
        Task SaveGroupLikesDataAsync(long groupId, DateTime from, DateTime to, IDictionary<long, int> data, bool overrideInCurrentDay);
    }
}
