using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Talabat.Admin.Localization;
using Talabat.Admin.Models;
using Talabat.Admin.Security;
using Talabat.Core.Entities.Identity;

namespace Talabat.Admin.Controllers
{
	[Authorize]
	[AdminPermission]
	public class ProfileController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly SharedLocalizer _L;

		public ProfileController(
			UserManager<AppUser> userManager,
			SignInManager<AppUser> signInManager,
			SharedLocalizer L)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_L = L;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();

			var roles = await _userManager.GetRolesAsync(user);
			var claims = await _userManager.GetClaimsAsync(user);
			var perms = claims
				.Where(c => c.Type == AdminPermissions.ClaimType)
				.Select(c => c.Value)
				.ToList();

			var vm = new ProfileViewModel
			{
				Email = user.Email ?? string.Empty,
				DisplayName = user.DisplayName ?? user.UserName ?? string.Empty,
				RoleLabel = roles.Contains(AdminRoles.Admin)
					? _L["admin.role.admin"]
					: _L["admin.role.staff"],
				Permissions = User.IsPanelAdmin()
					? AdminPermissions.All.Select(p => p.Key).ToList()
					: perms
			};

			ViewBag.ChangePassword = new ChangePasswordViewModel();
			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Challenge();

			if (!ModelState.IsValid)
			{
				TempData["Error"] = _L["admin.profile.passwordInvalid"];
				return RedirectToAction(nameof(Index));
			}

			var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
			if (!result.Succeeded)
			{
				TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
				return RedirectToAction(nameof(Index));
			}

			await _signInManager.RefreshSignInAsync(user);
			TempData["Success"] = _L["admin.profile.passwordChanged"];
			return RedirectToAction(nameof(Index));
		}
	}
}
