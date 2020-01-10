using System.Collections.Generic;

namespace Kitakun.VkModules.Web.WebModels
{
	public class Top100BestLikersModel
	{
		public IDictionary<long, int> Likes;

		public long[] Top100;

		public IDictionary<long, (string imgUrl, string uName)> UsersInfo;

		public bool IsAdmin = false;

        public string TopUsersTitleText = "Лучшие";
	}
}
