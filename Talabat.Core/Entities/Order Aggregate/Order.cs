using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entities.Order_Aggregate
{
   public class Order:BaseEntity
    {
		public Order()
		{
			
		}
		public Order(string buyerEmail, Address shippingAddress, DeliveryMethod deliveryMethod, ICollection<OrderItem> items, decimal subtotal,string paymentIntentId, string orderNumber)
		{
			BuyerEmail = buyerEmail;
			ShippingAddress = shippingAddress;
			DeliveryMethod = deliveryMethod;
			Items = items;
			Subtotal = subtotal;
			PaymentIntentId = paymentIntentId;
			OrderNumber = orderNumber;
		}

		public string BuyerEmail { get; set; }
		public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;
		public OrderStatus Status { get; set; } = OrderStatus.Pending;
		public Address ShippingAddress { get; set; }
		public DeliveryMethod DeliveryMethod { get; set; }
		public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();

		public decimal Subtotal { get; set; }
		public decimal GetTotal()
			=> Subtotal + DeliveryMethod.Cost;

		public string PaymentIntentId { get; set; }

		/// <summary>Public customer-facing code (not the database Id), e.g. TLB-261028-A7K2.</summary>
		public string OrderNumber { get; set; } = string.Empty;
	}
}
