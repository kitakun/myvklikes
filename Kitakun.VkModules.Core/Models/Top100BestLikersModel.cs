namespace Kitakun.VkModules.Core.Models
{
    using System.Collections.Generic;

    public class Top100BestLikersModel
    {
        public IDictionary<long, int> Likes;

        public long[] Top100;

        public IDictionary<long, (string imgUrl, string uName)> UsersInfo;

        public string TopUsersTitleText = "Лучшие";

        public string GroupUrlId;
    }
}
