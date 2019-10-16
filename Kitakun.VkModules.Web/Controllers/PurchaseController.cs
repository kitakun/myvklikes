namespace Kitakun.VkModules.Web.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Cors;
	using Microsoft.AspNetCore.Mvc;

	using Kitakun.VkModules.Web.Infrastructure;

	[Authorize]
	[EnableCors(WebConstants.AllCorsName)]
	public class PurchaseController : Controller
	{
	}
}
