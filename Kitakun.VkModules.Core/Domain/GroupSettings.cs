namespace Kitakun.VkModules.Core.Domain
{
    using System.ComponentModel.DataAnnotations;

    public class GroupSettings : IEntity
    {
        public int Id { get; set; }

        [Display(Name = "GroupId - Vk ID группы для настроек", Description = "test")]
        public long? GroupId { get; set; }

        [Display(Name = "ReverseGroup - Иногда Vk передает группу (например 15), а по факту нужно обращаться к -15")]
        public bool ReverseGroup { get; set; }

        [Display(Name = "TopLikersHeaderMessage - Текст заголовка лучших лайкателей")]
        public string TopLikersHeaderMessage { get; set; }

        [Display(Name = "GroupAppToken - Апи ключ группы, для фонового обновления виджета")]
        public string GroupAppToken { get; set; }

        [Display(Name = "RecuringBackgroundJobId - Системное, не трогать")]
        public string RecuringBackgroundJobId { get; set; }

        [Display(Name = "LastRunnedJobId - Системное, не трогать")]
        public string LastRunnedJobId { get; set; }

        public BackgroundUpdaterType BackgroundJobType { get; set; }

        [Display(Name = "LikePrice - число баллов за лайк поста")]
        public int LikePrice { get; set; }

        [Display(Name = "CommentPrice - число баллов за коммент к посту")]
        public int CommentPrice { get; set; }

        [Display(Name = "RepostPrice - число баллов за репост поста")]
        public int RepostPrice { get; set; }
    }
}
