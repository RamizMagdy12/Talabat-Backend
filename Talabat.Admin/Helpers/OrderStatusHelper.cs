using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.Admin.Helpers
{
	public static class OrderStatusHelper
	{
		public static string DisplayName(OrderStatus status) => status switch
		{
			OrderStatus.Pending => "Pending",
			OrderStatus.PaymentReceived => "Payment Received",
			OrderStatus.PaymentFailed => "Payment Failed",
			OrderStatus.Preparing => "Preparing",
			OrderStatus.OutForDelivery => "Out For Delivery",
			OrderStatus.Delivered => "Delivered",
			_ => status.ToString()
		};

		public static string BadgeClass(OrderStatus status) => status switch
		{
			OrderStatus.PaymentReceived => "badge-paid",
			OrderStatus.PaymentFailed => "badge-failed",
			OrderStatus.Preparing => "badge-prep",
			OrderStatus.OutForDelivery => "badge-ship",
			OrderStatus.Delivered => "badge-delivered",
			_ => "badge-pending"
		};
	}
}
