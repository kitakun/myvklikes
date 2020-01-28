namespace Kitakun.VkModules.Web.Services
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;

    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Persistance;

    public class WebContext : IWebContext
    {
        const int ultraAdmin1 = 45711035;
        const int ultraAdmin2 = 37825954;

        private readonly Lazy<bool> _isAdmin = null;
        private readonly Lazy<bool> _isUltraAdmin = null;
        private readonly Lazy<long> _groupId = null;
        private readonly Lazy<string> _vkAccessToken = null;
        private readonly Lazy<long> _actualUserId = null;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IVkDbContext _dbContext;
        private readonly ISessionSecretGenerator _sessionSecret;
        private readonly Lazy<IGroupLikesService> _lazyGroupLikesService;
        private readonly IMemoryCache _memoryCache;

        public bool IsAdmin => _isAdmin.Value;
        public bool IsUltraAdmin => _isUltraAdmin.Value;
        public long GroupId => _groupId.Value;
        public string VkAccessToken => _vkAccessToken.Value;

        public WebContext(
            IHttpContextAccessor httpContextAccessor,
            IVkDbContext dbContext,
            ISessionSecretGenerator sessionSecret,
            Lazy<IGroupLikesService> lazyGroupLikesService,
            IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _sessionSecret = sessionSecret ?? throw new ArgumentNullException(nameof(sessionSecret));
            _lazyGroupLikesService = lazyGroupLikesService ?? throw new ArgumentNullException(nameof(lazyGroupLikesService));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

            _vkAccessToken = new Lazy<string>(() =>
            {
                if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("access_token", out var accToken))
                {
                    return accToken[0].ToString();
                }

                return string.Empty;
            });

            _actualUserId = new Lazy<long>(() => _memoryCache.GetOrCreate($"VkAccessToken={VkAccessToken}", (e) =>
            {
                if (!string.IsNullOrEmpty(VkAccessToken))
                {
                    var vkaccess = _lazyGroupLikesService.Value.GetApi(VkAccessToken).GetAwaiter().GetResult();
                    var user = vkaccess.Users.Get(Enumerable.Empty<long>());
                    return user.FirstOrDefault()?.Id ?? 0;
                }
                return 0;
            }));

            _isAdmin = new Lazy<bool>(() =>
            {
                var viewerId = _actualUserId.Value;

                return _memoryCache.GetOrCreate($"IsViewerAdmin={viewerId}", (e) =>
                {
                    if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("group_id", out var groupIds))
                    {
                        var groupId = long.Parse(groupIds[0]);

                        if (viewerId == ultraAdmin1 || viewerId == ultraAdmin2)
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
            });

            _isUltraAdmin = new Lazy<bool>(() =>
            {
                var viewerId = _actualUserId.Value;

                if (IsSecretProtected())
                {
                    return viewerId == ultraAdmin1 || viewerId == ultraAdmin2;
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
