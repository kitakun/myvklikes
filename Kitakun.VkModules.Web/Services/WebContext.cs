namespace Kitakun.VkModules.Web.Services
{
    using System;
    using System.Linq;
    using System.Text;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;

    using Kitakun.VkModules.Services.Abstractions;
    using Kitakun.VkModules.Persistance;

    public class WebContext : IWebContext
    {
        const int ultraAdmin1 = 45711035;
        const int ultraAdmin2 = 37825954;

        private readonly Lazy<bool> _isAdmin = null;
        private readonly Lazy<bool> _isUltraAdmin = null;
        private readonly Lazy<long> _groupId = null;
        private readonly Lazy<bool> _isVkFrameValid = null;
        private readonly Lazy<long> _actualUserId = null;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IVkDbContext _dbContext;
        private readonly ISessionSecretGenerator _sessionSecret;
        private readonly Lazy<IGroupLikesService> _lazyGroupLikesService;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _config;

        public bool IsAdmin => _isAdmin.Value;
        public bool IsUltraAdmin => _isUltraAdmin.Value;
        public long GroupId => _groupId.Value;
        public bool IsVkFrameValid => _isVkFrameValid.Value;

        public WebContext(
            IHttpContextAccessor httpContextAccessor,
            IVkDbContext dbContext,
            ISessionSecretGenerator sessionSecret,
            Lazy<IGroupLikesService> lazyGroupLikesService,
            IMemoryCache memoryCache,
            IConfiguration config
            )
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _sessionSecret = sessionSecret ?? throw new ArgumentNullException(nameof(sessionSecret));
            _lazyGroupLikesService = lazyGroupLikesService ?? throw new ArgumentNullException(nameof(lazyGroupLikesService));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _isVkFrameValid = new Lazy<bool>(() =>
            {
                var fine = _httpContextAccessor.HttpContext.Request.Query.TryGetValue("api_id", out var apiIdQuery);
                fine |= _httpContextAccessor.HttpContext.Request.Query.TryGetValue("viewer_id", out var viewerIdQuery);
                fine |= _httpContextAccessor.HttpContext.Request.Query.TryGetValue("auth_key", out var authKeyQuery);

                if (fine)
                {
                    var md5 = CreateMD5($"{apiIdQuery}_{viewerIdQuery}_{_config.GetValue<string>("VkAppSecret")}");

                    return authKeyQuery.ToString().Equals(md5, StringComparison.OrdinalIgnoreCase);
                }

                return false;
            });

            _actualUserId = new Lazy<long>(() => _memoryCache.GetOrCreate($"VkAccessToken={IsVkFrameValid}", (e) =>
            {
                if (IsVkFrameValid && _httpContextAccessor.HttpContext.Request.Query.TryGetValue("viewer_id", out var viewerIdQuery))
                {
                    return long.Parse(viewerIdQuery);
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

        private static string CreateMD5(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
