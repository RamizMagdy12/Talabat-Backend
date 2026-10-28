using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.Admin.Models
{
	public class FinanceViewModel
	{
		public string? Search { get; set; }

		public decimal RevenueToday { get; set; }
		public decimal RevenueThisMonth { get; set; }
		public decimal RevenueAllTime { get; set; }

		public int PaidOrdersCount { get; set; }
		public decimal PendingAmount { get; set; }
		public int PendingOrdersCount { get; set; }
		public decimal FailedAmount { get; set; }
		public int FailedOrdersCount { get; set; }

		public decimal PreparingAmount { get; set; }
		public decimal OutForDeliveryAmount { get; set; }
		public decimal DeliveredAmount { get; set; }

		public IReadOnlyList<FinanceOrderRow> RecentOrders { get; set; } = Array.Empty<FinanceOrderRow>();
	}

	public class FinanceOrderRow
	{
		public int Id { get; set; }
		public string OrderNumber { get; set; } = string.Empty;
		public string BuyerEmail { get; set; } = string.Empty;
		public DateTimeOffset OrderDate { get; set; }
		public OrderStatus Status { get; set; }
		public decimal Total { get; set; }
		public bool CountsAsRevenue { get; set; }
	}
}
