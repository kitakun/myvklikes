namespace Kitakun.VkModules.Services.GroupLikeService
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    using VkNet;
    using VkNet.Enums.Filters;
    using VkNet.Model.RequestParams;
    using VkNet.Model;
    using VkNet.Enums.SafetyEnums;
    using VkNet.Model.Attachments;

    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Services.GroupLikeService.Models;

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
            DateTime to,
            Core.Domain.GroupSettings groupSetting)
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
                            PostOwnerId = curPost.OwnerId,
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

            if (groupSetting.LikePrice > 0 && allPosts.Count > 0)
            {
                // Likes
                var postsIdsWithLikes = CreatePostsIds(allPosts, true, false);

                if (postsIdsWithLikes.Length > 0)
                {
                    var loadPostsTask = new Task<VkNet.Utils.VkCollection<long>>[postsIdsWithLikes.Length];
                    for (var i = 0; i < postsIdsWithLikes.Length; i++)
                    {
                        loadPostsTask[i] = api.Likes.GetListAsync(new LikesGetListParams
                        {
                            OwnerId = userId,
                            Type = LikeObjectType.Post,
                            ItemId = postsIdsWithLikes[i],
                            Extended = true
                        });
                    }

                    await Task.WhenAll(loadPostsTask);

                    FillDictionaryWithLikes(ref linkedUserWithLikesCount, loadPostsTask, groupSetting.LikePrice);
                }

                // Comments
                var postsIdsWithComments = CreatePostsIds(allPosts, false, true);

                if (groupSetting.CommentPrice > 0 && postsIdsWithComments.Length > 0)
                {
                    var loadPostCommentsTask = new Task<WallGetCommentsResult>[postsIdsWithComments.Length];
                    for (var i = 0; i < postsIdsWithComments.Length; i++)
                    {
                        loadPostCommentsTask[i] = api.Wall.GetCommentsAsync(new WallGetCommentsParams
                        {
                            OwnerId = userId,
                            PreviewLength = 1,
                            Extended = false,
                            PostId = postsIdsWithComments[i]
                        });
                    }

                    await Task.WhenAll(loadPostCommentsTask);

                    FillDictionaryWithComments(ref linkedUserWithLikesCount, loadPostCommentsTask, groupSetting.CommentPrice);
                }

                // Reposts
                var postWithReposts = CreatePostsIdsForReposts(allPosts);

                // Not tested
                if (groupSetting.RepostPrice > 0 && postWithReposts.Length > 0)
                {
                    var loadReposts = new Task<WallGetObject>[postWithReposts.Length];

                    for (var i = 0; i < postWithReposts.Length; i++)
                    {
                        loadReposts[i] = api.Wall.GetRepostsAsync(postWithReposts[i].ownerId, postWithReposts[i].postId, null, null, false);
                    }

                    await Task.WhenAll(loadReposts);

                    FillDictionaryWithReposts(ref linkedUserWithLikesCount, loadReposts, groupSetting.RepostPrice);
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

        private static long[] CreatePostsIds(List<LoadWallPostsStoreProcedureRecord> allPosts, bool withLikes, bool withComments)
        {
            var preCount = 0;
            for (var i = 0; i < allPosts.Count; i++)
            {
                if (withLikes && allPosts[i].Likes > 0)
                {
                    preCount++;
                }
                else if (withComments && allPosts[i].Comments > 0)
                {
                    preCount++;
                }
            }
            var resultArray = new long[preCount];
            var index = 0;
            for (var i = 0; i < allPosts.Count; i++)
            {
                if (withLikes && allPosts[i].Likes > 0)
                {
                    resultArray[index] = allPosts[i].Id;
                    index++;
                }
                else if (withComments && allPosts[i].Comments > 0)
                {
                    resultArray[index] = allPosts[i].Id;
                    index++;
                }
            }
            return resultArray;
        }

        private static (long postId, long ownerId)[] CreatePostsIdsForReposts(List<LoadWallPostsStoreProcedureRecord> allPosts)
        {
            var preCount = 0;
            for (var i = 0; i < allPosts.Count; i++)
            {
                if (allPosts[i].PostOwnerId.HasValue && allPosts[i].Reposts > 0)
                {
                    preCount++;
                }
            }
            var resultArray = new (long postId, long ownerId)[preCount];
            var index = 0;
            for (var i = 0; i < allPosts.Count; i++)
            {
                if (allPosts[i].Reposts > 0)
                {
                    resultArray[index] = (allPosts[i].Id, allPosts[i].PostOwnerId.Value);
                    index++;
                }
            }
            return resultArray;
        }

        // Fill dictionary<userId, score> with score's 

        private static void FillDictionaryWithLikes(ref Dictionary<long, int> linkedUserWithLikesCount, Task<VkNet.Utils.VkCollection<long>>[] loadPostsTask, int likePrice)
        {
            for (var i = 0; i < loadPostsTask.Length; i++)
            {
                var taskResult = loadPostsTask[i].Result;
                for (var j = 0; j < taskResult.Count; j++)
                {
                    if (linkedUserWithLikesCount.ContainsKey(taskResult[j]))
                    {
                        linkedUserWithLikesCount[taskResult[j]] = linkedUserWithLikesCount[taskResult[j]] + likePrice;
                    }
                    else
                    {
                        linkedUserWithLikesCount.Add(taskResult[j], likePrice);
                    }
                }
            }
        }

        private static void FillDictionaryWithComments(ref Dictionary<long, int> linkedUserWithLikesCount, Task<WallGetCommentsResult>[] loadedCommentsTasks, int commentPrice)
        {
            for (var i = 0; i < loadedCommentsTasks.Length; i++)
            {
                var commentObject = loadedCommentsTasks[i].Result.Items;
                for (var j = 0; j < commentObject.Count; j++)
                {
                    if (!commentObject[j].FromId.HasValue)
                        continue;

                    var writerId = commentObject[j].FromId.Value;
                    if (linkedUserWithLikesCount.ContainsKey(writerId))
                    {
                        linkedUserWithLikesCount[writerId] = linkedUserWithLikesCount[writerId] + commentPrice;
                    }
                    else
                    {
                        linkedUserWithLikesCount.Add(writerId, commentPrice);
                    }
                }
            }
        }

        private static void FillDictionaryWithReposts(ref Dictionary<long, int> linkedUserWithLikesCount, Task<WallGetObject>[] repostsObject, int repostPrice)
        {
            for (var i = 0; i < repostsObject.Length; i++)
            {
                var repostObject = repostsObject[i].Result.Profiles;
                for (var j = 0; j < repostObject.Count; j++)
                {
                    var reposterId = repostObject[j].Id;
                    if (linkedUserWithLikesCount.ContainsKey(reposterId))
                    {
                        linkedUserWithLikesCount[reposterId] = linkedUserWithLikesCount[reposterId] + repostPrice;
                    }
                    else
                    {
                        linkedUserWithLikesCount.Add(reposterId, repostPrice);
                    }
                }
            }
        }
    }
}
