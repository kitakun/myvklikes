namespace Kitakun.VkModules.Services.GroupLikeService
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	using VkNet;
	using VkNet.Enums.Filters;
	using VkNet.Model.RequestParams;
	using VkNet.Model;
	using VkNet.Enums.SafetyEnums;
	using VkNet.Model.Attachments;

	using Kitakun.VkModules.Services.Abstractions;

    // https://vk.com/dev/objects/appWidget
    internal class GroupLikesService : IGroupLikesService
	{
		private VkApi _sharedVkApi;
		private string _sharedForToken;

		public async Task<VkApi> GetApi(string token)
		{
			if(_sharedVkApi != null && _sharedForToken == token)
			{
				return _sharedVkApi;
			}

			var api = new VkApi();

			await api.AuthorizeAsync(new ApiAuthParams
			{
				AccessToken = token,
				Settings = Settings.All
			});
			_sharedForToken = token;
			_sharedVkApi = api;

            return api;
		}

		public async Task<IDictionary<long, int>> LoadAllLikesForCommunityPostsAsync(
			string token,
			long userId,
			DateTime from,
			DateTime to)
		{
			var api = await GetApi(token);

			var allPosts = new List<Post>();

            const ulong loadPerPart = 100;

			var lastLoadedPosts = await api.Wall.GetAsync(new WallGetParams
			{
				Count = loadPerPart,
				OwnerId = userId,
				Extended = false
			});
			//validating first recieve posts
			var firstPosts = lastLoadedPosts.WallPosts.Where(a => a.Date > from && a.Date < to);
			allPosts.AddRange(firstPosts);

			ulong loadedCount = 0;

			var loadMore = lastLoadedPosts.TotalCount > loadPerPart;
			while (loadMore)
			{
				loadedCount += loadPerPart;
				lastLoadedPosts = await api.Wall.GetAsync(new WallGetParams
				{
					Count = loadPerPart,
					OwnerId = userId,
					Extended = false,
                    Offset = loadedCount
				});
				allPosts.AddRange(lastLoadedPosts.WallPosts.Where(a => a.Date > from && a.Date < to));
				loadMore = lastLoadedPosts.WallPosts.Count > 0 
                    && !lastLoadedPosts.WallPosts.Any(a => a.Date >= from && a.Date <= to) || lastLoadedPosts.TotalCount > loadedCount;
			}

			var linkedUserWithLikesCount = new Dictionary<long, int>();

			if (allPosts.Count > 0)
			{
				var postsIdsWithLikes = allPosts
					.Where(w => w.Likes.Count > 0 && w.Id.HasValue)
					.Select(s => s.Id.Value)
                    .ToArray();

				var loadPostsTask = postsIdsWithLikes
					.Select(postId => api.Likes.GetListAsync(new LikesGetListParams
					{
						OwnerId = userId,
						Type = LikeObjectType.Post,
						ItemId = postId,
						Extended = true
					})).ToArray();

				await Task.WhenAll(loadPostsTask);

				linkedUserWithLikesCount = loadPostsTask
					.Select(s => s.Result)
					.SelectMany(s => s)
					.GroupBy(v => v)
					.ToDictionary(s => s.Key, v => v.Count());
			}

			return linkedUserWithLikesCount;
		}

		public async Task<IDictionary<long, (string imgUrl, string uName)>> GetUsersPhotoAndNames(string token, long[] userIds)
		{
			var api = await GetApi(token);

			IDictionary<long, (string imgUrl, string uName)> methodResponse = api
				.Users
				.Get(userIds, ProfileFields.Photo50)
				.Select(s => new
				{
					s.Id,
					s.Photo50,
					s.FirstName,
					s.LastName
				})
				.ToDictionary(k => k.Id, v => (imgUrl: v.Photo50.ToString(), uName: $"{v.FirstName} {v.LastName}"));

			return methodResponse;
		}
	}
}
