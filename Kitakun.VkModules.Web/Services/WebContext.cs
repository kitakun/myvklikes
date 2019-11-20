using System;
using System.Linq;

using Microsoft.AspNetCore.Http;

using Kitakun.VkModules.Services.Abstractions;
using Kitakun.VkModules.Persistance;

namespace Kitakun.VkModules.Web.Services
{
	public class WebContext : IWebContext
	{
		private readonly Lazy<bool> _isAdmin = null;
		private readonly Lazy<bool> _isUltraAdmin = null;

		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IVkDbContext _dbContext;

		public bool IsAdmin { get => _isAdmin.Value; }
		public bool IsUltraAdmin { get => _isUltraAdmin.Value; }

		public WebContext(
			IHttpContextAccessor httpContextAccessor,
			IVkDbContext dbContext)
		{
			_httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

			const int ultraAdmin = 45711035;

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
					&& long.TryParse(viewerIds[0], out var viewerId))
				{
					return viewerId == ultraAdmin;
				}

				return false;
			});
		}
	}
}
