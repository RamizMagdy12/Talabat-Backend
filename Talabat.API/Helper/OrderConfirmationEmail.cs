using System.Globalization;
using System.Net;
using System.Text;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.API.Helper
{
	public static class OrderConfirmationEmail
	{
		public static string BuildHtml(Order order)
		{
			var culture = CultureInfo.GetCultureInfo("en-US");
			var addr = order.ShippingAddress;
			var deliveryName = order.DeliveryMethod?.ShortName ?? "-";
			var deliveryCost = order.DeliveryMethod?.Cost ?? 0m;
			var total = order.GetTotal();

			var sb = new StringBuilder();
			sb.Append("<div style='font-family:Segoe UI,Arial,sans-serif;max-width:600px;margin:0 auto;padding:24px;background:#f3f5f9;color:#14181f'>");
			sb.Append("<div style='background:#fff;border-radius:16px;padding:28px;border:1px solid #e4e8ef'>");
			sb.Append("<div style='width:42px;height:42px;border-radius:12px;background:linear-gradient(145deg,#ff4d00,#d93d00);color:#fff;font-weight:800;text-align:center;line-height:42px;font-size:20px'>T</div>");
			sb.Append("<h1 style='margin:18px 0 6px;font-size:22px'>Order confirmed</h1>");
			sb.Append("<p style='margin:0 0 18px;color:#5b6573;line-height:1.5'>Thanks for your order. Here are the details for <strong>")
				.Append(WebUtility.HtmlEncode(order.OrderNumber))
				.Append("</strong>.</p>");

			sb.Append("<table style='width:100%;border-collapse:collapse;margin:0 0 18px;font-size:14px'>");
			sb.Append("<tr><td style='padding:8px 0;color:#5b6573'>Status</td><td style='padding:8px 0;text-align:right;font-weight:600'>")
				.Append(WebUtility.HtmlEncode(FormatStatus(order.Status)))
				.Append("</td></tr>");
			sb.Append("<tr><td style='padding:8px 0;color:#5b6573'>Date</td><td style='padding:8px 0;text-align:right'>")
				.Append(WebUtility.HtmlEncode(order.OrderDate.ToLocalTime().ToString("f", culture)))
				.Append("</td></tr>");
			sb.Append("<tr><td style='padding:8px 0;color:#5b6573'>Delivery</td><td style='padding:8px 0;text-align:right'>")
				.Append(WebUtility.HtmlEncode(deliveryName))
				.Append(" (")
				.Append(deliveryCost.ToString("C", culture))
				.Append(")</td></tr>");
			sb.Append("</table>");

			sb.Append("<h2 style='margin:0 0 10px;font-size:16px'>Items</h2>");
			sb.Append("<table style='width:100%;border-collapse:collapse;font-size:14px;margin-bottom:18px'>");
			sb.Append("<thead><tr style='background:#f7f8fb;text-align:left'>");
			sb.Append("<th style='padding:10px;border-radius:8px 0 0 8px'>Product</th>");
			sb.Append("<th style='padding:10px'>Qty</th>");
			sb.Append("<th style='padding:10px;text-align:right;border-radius:0 8px 8px 0'>Line</th>");
			sb.Append("</tr></thead><tbody>");

			foreach (var item in order.Items)
			{
				var name = item.Product?.ProductName ?? "Item";
				var line = item.Price * item.Quantity;
				sb.Append("<tr style='border-bottom:1px solid #eef1f6'>");
				sb.Append("<td style='padding:10px'>").Append(WebUtility.HtmlEncode(name)).Append("<div style='color:#5b6573;font-size:12px'>")
					.Append(item.Price.ToString("C", culture)).Append(" each</div></td>");
				sb.Append("<td style='padding:10px'>").Append(item.Quantity).Append("</td>");
				sb.Append("<td style='padding:10px;text-align:right;font-weight:600'>").Append(line.ToString("C", culture)).Append("</td>");
				sb.Append("</tr>");
			}

			sb.Append("</tbody></table>");

			sb.Append("<table style='width:100%;border-collapse:collapse;font-size:14px;margin-bottom:18px'>");
			sb.Append("<tr><td style='padding:6px 0;color:#5b6573'>Subtotal</td><td style='padding:6px 0;text-align:right'>")
				.Append(order.Subtotal.ToString("C", culture)).Append("</td></tr>");
			sb.Append("<tr><td style='padding:6px 0;color:#5b6573'>Delivery</td><td style='padding:6px 0;text-align:right'>")
				.Append(deliveryCost.ToString("C", culture)).Append("</td></tr>");
			sb.Append("<tr><td style='padding:10px 0 0;font-size:16px;font-weight:700'>Total</td><td style='padding:10px 0 0;text-align:right;font-size:16px;font-weight:700;color:#ff4d00'>")
				.Append(total.ToString("C", culture)).Append("</td></tr>");
			sb.Append("</table>");

			if (addr != null)
			{
				sb.Append("<h2 style='margin:0 0 8px;font-size:16px'>Shipping</h2>");
				sb.Append("<p style='margin:0;color:#5b6573;line-height:1.6'>");
				sb.Append(WebUtility.HtmlEncode($"{addr.FirstName} {addr.LastName}")).Append("<br/>");
				if (!string.IsNullOrWhiteSpace(addr.PhoneNumber))
					sb.Append(WebUtility.HtmlEncode(addr.PhoneNumber)).Append("<br/>");
				sb.Append(WebUtility.HtmlEncode(addr.Street)).Append("<br/>");
				sb.Append(WebUtility.HtmlEncode($"{addr.City}, {addr.Country}"));
				sb.Append("</p>");
			}

			sb.Append("<p style='margin:22px 0 0;color:#8a93a2;font-size:12px'>Talabat Store — this is an automated confirmation.</p>");
			sb.Append("</div></div>");
			return sb.ToString();
		}

		private static string FormatStatus(OrderStatus status) => status switch
		{
			OrderStatus.Pending => "Pending",
			OrderStatus.PaymentReceived => "Payment Received",
			OrderStatus.Preparing => "Preparing",
			OrderStatus.OutForDelivery => "Out for Delivery",
			OrderStatus.Delivered => "Delivered",
			OrderStatus.PaymentFailed => "Payment Failed",
			_ => status.ToString()
		};
	}
}
