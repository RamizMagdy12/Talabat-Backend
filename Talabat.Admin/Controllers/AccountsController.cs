using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Talabat.Admin.Localization;
using Talabat.Admin.Models;
using Talabat.Admin.Security;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Services.Contract;

namespace Talabat.Admin.Controllers
{
	[Authorize(Roles = AdminRoles.Admin)]
	public class AccountsController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IOrderService _orderService;
		private readonly SharedLocalizer _L;

		public AccountsController(
			UserManager<AppUser> userManager,
			RoleManager<IdentityRole> roleManager,
			IOrderService orderService,
			SharedLocalizer L)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_orderService = orderService;
			_L = L;
		}

		public async Task<IActionResult> Index(string? q)
		{
			var todayLocal = DateTime.Today;
			var todayUtcStart = todayLocal.ToUniversalTime();
			var tomorrowUtcStart = todayLocal.AddDays(1).ToUniversalTime();
			var monthStartLocal = new DateTime(todayLocal.Year, todayLocal.Month, 1);
			var monthStartUtc = monthStartLocal.ToUniversalTime();

			var users = await _userManager.Users.AsNoTracking().ToListAsync();
			var orders = await _orderService.GetAllOrdersAsync();

			var search = q?.Trim();
			if (!string.IsNullOrWhiteSpace(search))
			{
				users = users.Where(u =>
					(u.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
					(u.DisplayName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
					(u.UserName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
					(u.PhoneNumber?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
				).ToList();
			}

			var items = new List<AccountListItemViewModel>();
			foreach (var user in users.OrderBy(u => u.Email))
			{
				var email = user.Email ?? string.Empty;
				var userOrders = orders.Where(o =>
					string.Equals(o.BuyerEmail, email, StringComparison.OrdinalIgnoreCase)).ToList();
				var roles = (await _userManager.GetRolesAsync(user)).ToList();

				items.Add(new AccountListItemViewModel
				{
					Id = user.Id,
					DisplayName = user.DisplayName ?? user.UserName ?? email,
					Email = email,
					UserName = user.UserName ?? string.Empty,
					PhoneNumber = user.PhoneNumber,
					Roles = roles,
					OrdersToday = userOrders.Count(o => o.OrderDate >= todayUtcStart && o.OrderDate < tomorrowUtcStart),
					OrdersThisMonth = userOrders.Count(o => o.OrderDate >= monthStartUtc),
					TotalOrders = userOrders.Count
				});
			}

			var vm = new AccountsIndexViewModel
			{
				Search = search,
				TotalAccounts = items.Count,
				OrdersToday = orders.Count(o => o.OrderDate >= todayUtcStart && o.OrderDate < tomorrowUtcStart),
				OrdersThisMonth = orders.Count(o => o.OrderDate >= monthStartUtc),
				Accounts = items
			};

			return View(vm);
		}

		[HttpGet]
		public IActionResult Create() => View(new CreateStaffViewModel());

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CreateStaffViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			await EnsureRolesAsync();

			var existing = await _userManager.FindByEmailAsync(model.Email.Trim());
			if (existing != null)
			{
				ModelState.AddModelError(nameof(model.Email), _L["admin.users.emailTaken"]);
				return View(model);
			}

			var userName = model.Email.Trim().Split('@')[0];
			var baseName = userName;
			var n = 1;
			while (await _userManager.FindByNameAsync(userName) != null)
				userName = $"{baseName}{n++}";

			var user = new AppUser
			{
				DisplayName = model.DisplayName.Trim(),
				Email = model.Email.Trim(),
				UserName = userName,
				EmailConfirmed = true
			};

			var create = await _userManager.CreateAsync(user, model.Password);
			if (!create.Succeeded)
			{
				foreach (var err in create.Errors)
					ModelState.AddModelError(string.Empty, err.Description);
				return View(model);
			}

			if (model.IsAdmin)
			{
				await _userManager.AddToRoleAsync(user, AdminRoles.Admin);
				TempData["Success"] = _L.Get("admin.users.adminCreated", ("email", user.Email!));
			}
			else
			{
				await _userManager.AddToRoleAsync(user, AdminRoles.Staff);
				var perms = NormalizePermissions(model.SelectedPermissions);
				if (perms.Count == 0)
					perms.Add(AdminPermissions.Dashboard);

				foreach (var p in perms)
					await _userManager.AddClaimAsync(user, new Claim(AdminPermissions.ClaimType, p));

				TempData["Success"] = _L.Get("admin.users.staffCreated", ("email", user.Email!));
			}

			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		public async Task<IActionResult> EditPermissions(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null) return NotFound();

			var roles = await _userManager.GetRolesAsync(user);
			var isAdmin = roles.Contains(AdminRoles.Admin);
			var isStaff = roles.Contains(AdminRoles.Staff);
			if (!isAdmin && !isStaff)
			{
				TempData["Error"] = _L["admin.users.notStaff"];
				return RedirectToAction(nameof(Index));
			}

			var claims = await _userManager.GetClaimsAsync(user);
			var vm = new EditStaffPermissionsViewModel
			{
				UserId = user.Id,
				Email = user.Email ?? string.Empty,
				DisplayName = user.DisplayName ?? string.Empty,
				IsAdmin = isAdmin,
				SelectedPermissions = claims
					.Where(c => c.Type == AdminPermissions.ClaimType)
					.Select(c => c.Value)
					.ToList()
			};
			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditPermissions(EditStaffPermissionsViewModel model)
		{
			var user = await _userManager.FindByIdAsync(model.UserId);
			if (user == null) return NotFound();

			await EnsureRolesAsync();

			var roles = await _userManager.GetRolesAsync(user);
			var currentUser = await _userManager.GetUserAsync(User);
			var isSelf = currentUser != null && currentUser.Id == user.Id;

			if (model.IsAdmin)
			{
				if (!roles.Contains(AdminRoles.Admin))
					await _userManager.AddToRoleAsync(user, AdminRoles.Admin);
				if (roles.Contains(AdminRoles.Staff))
					await _userManager.RemoveFromRoleAsync(user, AdminRoles.Staff);

				var oldClaims = (await _userManager.GetClaimsAsync(user))
					.Where(c => c.Type == AdminPermissions.ClaimType)
					.ToList();
				foreach (var c in oldClaims)
					await _userManager.RemoveClaimAsync(user, c);
			}
			else
			{
				if (isSelf && roles.Contains(AdminRoles.Admin))
				{
					TempData["Error"] = _L["admin.users.cannotDemoteSelf"];
					return RedirectToAction(nameof(EditPermissions), new { id = model.UserId });
				}

				if (roles.Contains(AdminRoles.Admin))
					await _userManager.RemoveFromRoleAsync(user, AdminRoles.Admin);
				if (!roles.Contains(AdminRoles.Staff))
					await _userManager.AddToRoleAsync(user, AdminRoles.Staff);

				var oldClaims = (await _userManager.GetClaimsAsync(user))
					.Where(c => c.Type == AdminPermissions.ClaimType)
					.ToList();
				foreach (var c in oldClaims)
					await _userManager.RemoveClaimAsync(user, c);

				var perms = NormalizePermissions(model.SelectedPermissions);
				if (perms.Count == 0)
					perms.Add(AdminPermissions.Dashboard);
				foreach (var p in perms)
					await _userManager.AddClaimAsync(user, new Claim(AdminPermissions.ClaimType, p));
			}

			TempData["Success"] = _L["admin.users.permissionsUpdated"];
			return RedirectToAction(nameof(Index));
		}

		private static List<string> NormalizePermissions(IEnumerable<string>? selected)
		{
			var allowed = AdminPermissions.All.Select(p => p.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
			return (selected ?? Enumerable.Empty<string>())
				.Where(p => allowed.Contains(p))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		private async Task EnsureRolesAsync()
		{
			if (!await _roleManager.RoleExistsAsync(AdminRoles.Admin))
				await _roleManager.CreateAsync(new IdentityRole(AdminRoles.Admin));
			if (!await _roleManager.RoleExistsAsync(AdminRoles.Staff))
				await _roleManager.CreateAsync(new IdentityRole(AdminRoles.Staff));
		}
	}
}
