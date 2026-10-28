using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talabat.Admin.Models;
using Talabat.Admin.Security;

namespace Talabat.Admin.Controllers
{
	[Authorize]
	[AdminPermission]
	public class HomeController : Controller
	{
		public IActionResult Index() =>
			RedirectToAction("Index", User.HomeController());

		[AllowAnonymous]
		public IActionResult Privacy() => View();

		[AllowAnonymous]
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error() =>
			View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}
}
