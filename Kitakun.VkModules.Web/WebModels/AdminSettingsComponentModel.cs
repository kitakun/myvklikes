﻿namespace Kitakun.VkModules.Web.WebModels
{
    public class AdminSettingsComponentModel
    {
        public string AppToken { get; set; }

        public string TopLikersHeaderMessage { get; set; }

        public bool EnableAutoupdatingTop3 { get; set; }

        public bool EnableAutoupdatingTop5 { get; set; }

        public int PriceFromLike { get; set; }
        public int PriceFromComment { get; set; }
        public int PriceFromRepost { get; set; }
    }
}
