using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talabat.Admin.Models;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Services.Contract;

namespace Talabat.Admin.Controllers
{
	[Authorize(Roles = "Admin")]
	public class DashboardController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IOrderService _orderService;

		public DashboardController(IUnitOfWork unitOfWork, IOrderService orderService)
		{
			_unitOfWork = unitOfWork;
			_orderService = orderService;
		}

		public async Task<IActionResult> Index()
		{
			var products = await _unitOfWork.Repository<Product>().GetAllAsync();
			var orders = await _orderService.GetAllOrdersAsync();

			var todayLocal = DateTime.Today;
			var todayUtcStart = todayLocal.ToUniversalTime();
			var tomorrowUtcStart = todayLocal.AddDays(1).ToUniversalTime();
			var monthStartUtc = new DateTime(todayLocal.Year, todayLocal.Month, 1).ToUniversalTime();

			var vm = new DashboardViewModel
			{
				TotalProducts = products.Count,
				LowStockCount = products.Count(p => p.Stock <= 10),
				PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
				PaidOrders = orders.Count(o => o.Status == OrderStatus.PaymentReceived),
				PreparingOrders = orders.Count(o => o.Status == OrderStatus.Preparing),
				OutForDeliveryOrders = orders.Count(o => o.Status == OrderStatus.OutForDelivery),
				DeliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
				OrdersToday = orders.Count(o => o.OrderDate >= todayUtcStart && o.OrderDate < tomorrowUtcStart),
				OrdersThisMonth = orders.Count(o => o.OrderDate >= monthStartUtc),
				RecentOrders = orders.Take(8).ToList()
			};

			return View(vm);
		}
	}
}
