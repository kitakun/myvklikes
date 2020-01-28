namespace Kitakun.VkModules.Web.Extensions
{
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Http;

    public static class QueryExtensions
    {
        /// <summary>
        /// Move all query parameters to current request
        /// </summary>
        public static Dictionary<string, string> ToQuery(this HttpRequest request)
        {
            var query = new Dictionary<string, string>();
            foreach (var pair in request.Query)
            {
                query.Add(pair.Key, pair.Value);
            }
            return query;
        }
    }
}
