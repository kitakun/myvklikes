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
    using Kitakun.VkModules.Services.GroupLikeService.Models;
    using System.Collections.ObjectModel;

    // https://vk.com/dev/objects/appWidget
    internal class GroupLikesService : IGroupLikesService
    {
        private const ulong loadPerPart = 100;

        private VkApi _sharedVkApi;
        private string _sharedForToken;

        public async Task<VkApi> GetApi(string token)
        {
            if (_sharedVkApi != null && _sharedForToken == token)
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

            var allPosts = new List<LoadWallPostsStoreProcedureRecord>(64);

            var lastLoadedPosts = await api.Wall.GetAsync(new WallGetParams
            {
                Count = loadPerPart,
                OwnerId = userId,
                Extended = false
            });

            //validating first recieve posts
            proceedPostsToAllPostsCollection(lastLoadedPosts.WallPosts);

            ulong loadedCount = 0;

            void proceedPostsToAllPostsCollection(ReadOnlyCollection<Post> posts)
            {
                for (var i = 0; i < posts.Count; i++)
                {
                    var curPost = posts[i];
                    if (curPost.Date > from && curPost.Date < to)
                    {
                        allPosts.Add(new LoadWallPostsStoreProcedureRecord
                        {
                            Id = curPost.Id.Value,
                            Likes = curPost.Likes.Count,
                            Comments = curPost.Comments.Count,
                            Reposts = curPost.Reposts.Count
                        });
                    }
                }
            }

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

                proceedPostsToAllPostsCollection(lastLoadedPosts.WallPosts);

                loadMore = lastLoadedPosts.WallPosts.Count > 0;
                if (loadMore)
                {
                    for (var i = 0; i < lastLoadedPosts.WallPosts.Count; i++)
                    {
                        var post = lastLoadedPosts.WallPosts[i];
                        var anyCondition = post.Date >= from && post.Date <= to
                            || lastLoadedPosts.TotalCount > loadedCount;
                        loadMore = !anyCondition;
                        if (!loadMore)
                            break;
                    }
                }
            }

            var linkedUserWithLikesCount = new Dictionary<long, int>();

            if (allPosts.Count > 0)
            {
                var postsIdsWithLikes = CreatePostsIdsWithLikes(allPosts);

                if (postsIdsWithLikes.Length > 0)
                {
                    var loadPostsTask = postsIdsWithLikes
                        .Select(postId => api.Likes.GetListAsync(new LikesGetListParams
                        {
                            OwnerId = userId,
                            Type = LikeObjectType.Post,
                            ItemId = postId,
                            Extended = true
                        })).ToArray();

                    await Task.WhenAll(loadPostsTask);

                    FillDictionaryWithLikes(ref linkedUserWithLikesCount, loadPostsTask);
                }
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

        private static long[] CreatePostsIdsWithLikes(List<LoadWallPostsStoreProcedureRecord> allPosts)
        {
            var preCount = 0;
            for (var i = 0; i < allPosts.Count; i++)
            {
                if (allPosts[i].Likes > 0)
                {
                    preCount++;
                }
            }
            var resultArray = new long[preCount];
            var index = 0;
            for (var i = 0; i < allPosts.Count; i++)
            {
                if (allPosts[i].Likes > 0)
                {
                    resultArray[index] = allPosts[i].Id;
                }
                index++;
            }
            return resultArray;
        }

        private static void FillDictionaryWithLikes(ref Dictionary<long, int> linkedUserWithLikesCount, Task<VkNet.Utils.VkCollection<long>>[] loadPostsTask)
        {
            for (var i = 0; i < loadPostsTask.Length; i++)
            {
                var taskResult = loadPostsTask[i].Result;
                for (var j = 0; j < taskResult.Count; j++)
                {
                    if (linkedUserWithLikesCount.ContainsKey(taskResult[j]))
                    {
                        linkedUserWithLikesCount[taskResult[j]] = linkedUserWithLikesCount[taskResult[j]] + 1; 
                    }
                    else
                    {
                        linkedUserWithLikesCount.Add(taskResult[j], 1);
                    }
                }
            }
        }
    }
}
