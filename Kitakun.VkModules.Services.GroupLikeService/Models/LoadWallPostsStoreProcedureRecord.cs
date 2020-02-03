namespace Kitakun.VkModules.Services.GroupLikeService.Models
{
    public struct LoadWallPostsStoreProcedureRecord
    {
        public long Id { get; set; }
        public long? PostOwnerId { get; set; }
        public long Likes { get; set; }
        public long Comments { get; set; }
        public long Reposts { get; set; }
    }
}
