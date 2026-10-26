using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talabat.Admin.Helpers;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Services.Contract;

namespace Talabat.Admin.Controllers
{
	[Authorize(Roles = "Admin")]
	public class OrdersController : Controller
	{
		private readonly IOrderService _orderService;

		public OrdersController(IOrderService orderService)
		{
			_orderService = orderService;
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
			return View(order);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
		{
			var updated = await _orderService.UpdateOrderStatusAsync(id, status);
			TempData[updated ? "Success" : "Error"] = updated
				? "Order status updated."
				: "Failed to update order status.";
			return RedirectToAction(nameof(Details), new { id });
		}
	}
}
