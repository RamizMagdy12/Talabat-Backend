using System.Globalization;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.Admin.Helpers
{
	public static class OrderStatusHelper
	{
		private static bool IsArabic =>
			CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase);

		public static string DisplayName(OrderStatus status) => status switch
		{
			OrderStatus.Pending => IsArabic ? "قيد الانتظار" : "Pending",
			OrderStatus.PaymentReceived => IsArabic ? "تم الدفع" : "Payment Received",
			OrderStatus.PaymentFailed => IsArabic ? "فشل الدفع" : "Payment Failed",
			OrderStatus.Preparing => IsArabic ? "قيد التحضير" : "Preparing",
			OrderStatus.OutForDelivery => IsArabic ? "خرج للتوصيل" : "Out For Delivery",
			OrderStatus.Delivered => IsArabic ? "تم التوصيل" : "Delivered",
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
