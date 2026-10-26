using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talabat.Admin.Helpers;
using Talabat.Admin.Models;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Services.Contract;

namespace Talabat.Admin.Controllers
{
	[Authorize(Roles = "Admin")]
	public class FinanceController : Controller
	{
		private static readonly HashSet<OrderStatus> RevenueStatuses =
		[
			OrderStatus.PaymentReceived,
			OrderStatus.Preparing,
			OrderStatus.OutForDelivery,
			OrderStatus.Delivered
		];

		private readonly IOrderService _orderService;

		public FinanceController(IOrderService orderService)
		{
			_orderService = orderService;
		}

		public async Task<IActionResult> Index(string? q)
		{
			var orders = await _orderService.GetAllOrdersAsync();
			var todayLocal = DateTime.Today;
			var todayUtcStart = todayLocal.ToUniversalTime();
			var tomorrowUtcStart = todayLocal.AddDays(1).ToUniversalTime();
			var monthStartUtc = new DateTime(todayLocal.Year, todayLocal.Month, 1).ToUniversalTime();

			decimal TotalOf(Order o) =>
				o.DeliveryMethod != null ? o.GetTotal() : o.Subtotal;

			bool IsRevenue(Order o) => RevenueStatuses.Contains(o.Status);

			var revenueOrders = orders.Where(IsRevenue).ToList();
			var pending = orders.Where(o => o.Status == OrderStatus.Pending).ToList();
			var failed = orders.Where(o => o.Status == OrderStatus.PaymentFailed).ToList();

			var filtered = orders.AsEnumerable();
			if (!string.IsNullOrWhiteSpace(q))
			{
				var term = q.Trim();
				filtered = filtered.Where(o =>
					o.Id.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
					(o.BuyerEmail?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
					OrderStatusHelper.DisplayName(o.Status).Contains(term, StringComparison.OrdinalIgnoreCase));
			}

			var vm = new FinanceViewModel
			{
				Search = q,
				RevenueToday = revenueOrders
					.Where(o => o.OrderDate >= todayUtcStart && o.OrderDate < tomorrowUtcStart)
					.Sum(TotalOf),
				RevenueThisMonth = revenueOrders
					.Where(o => o.OrderDate >= monthStartUtc)
					.Sum(TotalOf),
				RevenueAllTime = revenueOrders.Sum(TotalOf),
				PaidOrdersCount = revenueOrders.Count,
				PendingAmount = pending.Sum(TotalOf),
				PendingOrdersCount = pending.Count,
				FailedAmount = failed.Sum(TotalOf),
				FailedOrdersCount = failed.Count,
				PreparingAmount = orders.Where(o => o.Status == OrderStatus.Preparing).Sum(TotalOf),
				OutForDeliveryAmount = orders.Where(o => o.Status == OrderStatus.OutForDelivery).Sum(TotalOf),
				DeliveredAmount = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(TotalOf),
				RecentOrders = filtered
					.OrderByDescending(o => o.OrderDate)
					.Take(50)
					.Select(o => new FinanceOrderRow
					{
						Id = o.Id,
						BuyerEmail = o.BuyerEmail,
						OrderDate = o.OrderDate,
						Status = o.Status,
						Total = TotalOf(o),
						CountsAsRevenue = IsRevenue(o)
					})
					.ToList()
			};

			return View(vm);
		}
	}
}
