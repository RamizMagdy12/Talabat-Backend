using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Talabat.Admin.Helpers;
using Talabat.Admin.Localization;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Services.Contract;

using Talabat.Admin.Security;

namespace Talabat.Admin.Controllers
{
	[AdminPermission(AdminPermissions.Orders)]
	public class OrdersController : Controller
	{
		private readonly IOrderService _orderService;
		private readonly UserManager<AppUser> _userManager;
		private readonly SharedLocalizer _L;

		public OrdersController(IOrderService orderService, UserManager<AppUser> userManager, SharedLocalizer L)
		{
			_orderService = orderService;
			_userManager = userManager;
			_L = L;
		}

		public async Task<IActionResult> Index(string? q, OrderStatus? status)
		{
			var orders = (await _orderService.GetAllOrdersAsync()).AsEnumerable();

			if (status.HasValue)
				orders = orders.Where(o => o.Status == status.Value);

			if (!string.IsNullOrWhiteSpace(q))
			{
				var term = q.Trim();
				orders = orders.Where(o =>
					o.Id.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
					(o.BuyerEmail?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
					OrderStatusHelper.DisplayName(o.Status).Contains(term, StringComparison.OrdinalIgnoreCase) ||
					(o.PaymentIntentId?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
			}

			ViewBag.Search = q;
			ViewBag.Status = status;
			return View(orders.ToList());
		}

		public async Task<IActionResult> Details(int id)
		{
			var order = await _orderService.GetOrderByIdAsync(id);
			if (order == null) return NotFound();

			var phone = order.ShippingAddress?.PhoneNumber;
			if (string.IsNullOrWhiteSpace(phone) && !string.IsNullOrWhiteSpace(order.BuyerEmail))
			{
				var buyer = await _userManager.FindByEmailAsync(order.BuyerEmail);
				phone = buyer?.PhoneNumber;
			}

			ViewBag.BuyerPhone = phone ?? string.Empty;
			return View(order);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
		{
			var updated = await _orderService.UpdateOrderStatusAsync(id, status);
			TempData[updated ? "Success" : "Error"] = updated
				? _L["admin.msg.orderUpdated"]
				: _L["admin.msg.orderUpdateFailed"];
			return RedirectToAction(nameof(Details), new { id });
		}
	}
}
