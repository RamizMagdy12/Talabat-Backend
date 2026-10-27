using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Talabat.Admin.Controllers
{
	public class LanguageController : Controller
	{
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Set(string culture, string? returnUrl = null)
		{
			culture = culture is "ar" or "en" ? culture : "en";

			Response.Cookies.Append(
				CookieRequestCultureProvider.DefaultCookieName,
				CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
				new CookieOptions
				{
					Expires = DateTimeOffset.UtcNow.AddYears(1),
					IsEssential = true,
					Path = "/"
				});

			if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
				return LocalRedirect(returnUrl);

			return RedirectToAction("Index", "Dashboard");
		}
	}
}
