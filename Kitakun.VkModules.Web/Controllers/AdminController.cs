namespace Kitakun.VkModules.Web.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;

    using Kitakun.VkModules.Web.Infrastructure;
    using Kitakun.VkModules.Services.Abstractions;

    [AllowAnonymous]
    [EnableCors(WebConstants.AllCorsName)]
    public sealed class AdminController : Controller
    {
        private readonly IWebContext _webContext;
        private readonly ISessionSecretGenerator _sessionSecret;

        public AdminController(
            IWebContext webContext,
            ISessionSecretGenerator sessionSecret)
        {
            _webContext = webContext ?? throw new System.ArgumentNullException(nameof(webContext));
            _sessionSecret = sessionSecret ?? throw new System.ArgumentNullException(nameof(sessionSecret));
        }

        [HttpGet]
        public IActionResult GetSecret()
            => _webContext.IsAdmin
                ? Ok(_sessionSecret.GetSessionSecret()) as IActionResult
                : NotFound();
    }
}
