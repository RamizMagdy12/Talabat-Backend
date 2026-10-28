using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Talabat.Admin.Models;
using Talabat.Admin.Security;
using Talabat.Core.Entities.Identity;

namespace Talabat.Admin.Controllers
{
	public class AccountController : Controller
	{
		private readonly SignInManager<AppUser> _signInManager;
		private readonly UserManager<AppUser> _userManager;

		public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
		{
			_signInManager = signInManager;
			_userManager = userManager;
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult Login(string? returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			return View(new LoginViewModel());
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			if (!ModelState.IsValid) return View(model);

			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				ModelState.AddModelError(string.Empty, "Invalid login attempt.");
				return View(model);
			}

			var isAdmin = await _userManager.IsInRoleAsync(user, AdminRoles.Admin);
			var isStaff = await _userManager.IsInRoleAsync(user, AdminRoles.Staff);
			if (!isAdmin && !isStaff)
			{
				ModelState.AddModelError(string.Empty, "You are not authorized to access the admin panel.");
				return View(model);
			}

			var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
			if (!result.Succeeded)
			{
				ModelState.AddModelError(string.Empty, "Invalid login attempt.");
				return View(model);
			}

			if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
				return Redirect(returnUrl);

			// Fresh principal after sign-in for home routing
			var principal = await _signInManager.CreateUserPrincipalAsync(user);
			return RedirectToAction("Index", principal.HomeController());
		}

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction(nameof(Login));
		}

		[AllowAnonymous]
		public IActionResult AccessDenied() => View();
	}
}
