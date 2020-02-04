namespace Kitakun.VkModules.Services.Abstractions
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

    using VkNet;

    public interface IGroupLikesService
	{
        Task<VkApi> GetApi(string token);

        /// <summary>
        /// Load all likes for community with filter
        /// </summary>
        /// <param name="token">app service key</param>
        /// <param name="userId">user or group id</param>
        /// <param name="from">From datetime</param>
        /// <param name="to">To datetime</param>
        /// <returns>Dictionary[userId, likesCount]</returns>
        Task<IDictionary<long, int>> LoadAllLikesForCommunityPostsAsync(string token, long userId, DateTime from, DateTime to, Core.Domain.GroupSettings groupSetting);

        /// <summary>
        /// Load users small preview with names by userIds
        /// </summary>
        /// <param name="token">app token</param>
        /// <param name="userIds">User ids</param>
        /// <returns>Dictionary</returns>
		Task<IDictionary<long, (string imgUrl, string uName)>> GetUsersPhotoAndNames(string token, long[] userIds);
	}
}
