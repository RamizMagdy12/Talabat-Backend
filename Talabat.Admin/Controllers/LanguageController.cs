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
			var ui = culture is "ar" or "en" ? culture : "en";
			// Keep en-US for numbers/currency ($), Arabic only for UI strings.
			var requestCulture = new RequestCulture(culture: "en-US", uiCulture: ui);

			Response.Cookies.Append(
				CookieRequestCultureProvider.DefaultCookieName,
				CookieRequestCultureProvider.MakeCookieValue(requestCulture),
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
