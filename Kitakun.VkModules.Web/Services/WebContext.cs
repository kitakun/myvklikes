namespace Kitakun.VkModules.Web.Services
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Http;

    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Persistance;

    public class WebContext : IWebContext
    {
        const int ultraAdmin = 45711035;

        private readonly Lazy<bool> _isAdmin = null;
        private readonly Lazy<bool> _isUltraAdmin = null;
        private readonly Lazy<long> _groupId = null;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IVkDbContext _dbContext;
        private readonly ISessionSecretGenerator _sessionSecret;

        public bool IsAdmin => _isAdmin.Value;
        public bool IsUltraAdmin => _isUltraAdmin.Value;
        public long GroupId => _groupId.Value;

        public WebContext(
            IHttpContextAccessor httpContextAccessor,
            IVkDbContext dbContext,
            ISessionSecretGenerator sessionSecret)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _sessionSecret = sessionSecret ?? throw new ArgumentNullException(nameof(sessionSecret));

            _isAdmin = new Lazy<bool>(() =>
            {
                if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("viewer_id", out var viewerIds)
                    && _httpContextAccessor.HttpContext.Request.Query.TryGetValue("group_id", out var groupIds))
                {
                    var viewerId = long.Parse(viewerIds[0]);
                    var groupId = long.Parse(groupIds[0]);

                    if (viewerId == ultraAdmin)
                        return true;

                    var currentDate = DateTime.UtcNow.Date;

                    return _dbContext
                        .Subscriptions
                        .Any(a =>
                            a.UserId == viewerId
                            && a.GroupId == groupId
                            && a.From <= currentDate
                            && a.To >= currentDate);
                }

                return false;
            });

            _isUltraAdmin = new Lazy<bool>(() =>
            {
                if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("viewer_id", out var viewerIds)
                    && long.TryParse(viewerIds[0], out var viewerId)
                    && IsSecretProtected())
                {
                    return viewerId == ultraAdmin;
                }

                return false;
            });

            _groupId = new Lazy<long>(() =>
            {
                if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("group_id", out var strVal))
                {
                    return long.Parse(strVal);
                }
                return 0;
            });
        }

        private bool IsSecretProtected()
        {
            var secret = _sessionSecret.GetSessionSecret();

            if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("secret", out var secretData))
            {
                return secretData == secret;
            }

            return false;
        }
    }
}
