namespace Kitakun.VkModules.Web.Infrastructure
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Extensions.Configuration;

    using Hangfire.Annotations;
    using Hangfire.Dashboard;

    public class HangFireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private static string[] allowedClients = null;

        public HangFireAuthorizationFilter(IConfiguration appConfigs)
        {
            if (appConfigs == null)
            {
                throw new ArgumentNullException(nameof(appConfigs));
            }

            if (allowedClients == null)
            {
                var allowIps = new List<string>();
                appConfigs.GetSection("HangfireAccess").Bind(allowIps);
                allowedClients = allowIps.ToArray();
            }
        }

        public bool Authorize([NotNull] DashboardContext context)
        {
            var ria = context.Request.RemoteIpAddress;

            for (var i = 0; i < allowedClients.Length; i++)
            {
                if (allowedClients[i] == ria)
                    return true;
            }
            Console.WriteLine($"[HANGIFRE] IP={ria}");
            return false;
        }
    }
}