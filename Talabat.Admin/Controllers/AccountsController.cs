using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Talabat.Admin.Models;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Services.Contract;

namespace Talabat.Admin.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AccountsController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly IOrderService _orderService;

		public AccountsController(UserManager<AppUser> userManager, IOrderService orderService)
		{
			_userManager = userManager;
			_orderService = orderService;
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

				items.Add(new AccountListItemViewModel
				{
					Id = user.Id,
					DisplayName = user.DisplayName ?? user.UserName ?? email,
					Email = email,
					UserName = user.UserName ?? string.Empty,
					PhoneNumber = user.PhoneNumber,
					Roles = (await _userManager.GetRolesAsync(user)).ToList(),
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
	}
}
