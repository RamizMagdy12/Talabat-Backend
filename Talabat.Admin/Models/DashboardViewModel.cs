using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.Admin.Models
{
	public class DashboardViewModel
	{
		public int TotalProducts { get; set; }
		public int LowStockCount { get; set; }
		public int PendingOrders { get; set; }
		public int PaidOrders { get; set; }
		public int PreparingOrders { get; set; }
		public int OutForDeliveryOrders { get; set; }
		public int DeliveredOrders { get; set; }
		public int OrdersToday { get; set; }
		public int OrdersThisMonth { get; set; }
		public IReadOnlyList<Order> RecentOrders { get; set; } = Array.Empty<Order>();
	}
}
