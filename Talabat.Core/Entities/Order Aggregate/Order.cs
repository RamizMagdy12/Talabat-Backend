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
		public Order(string buyerEmail, Address shippingAddress, DeliveryMethod deliveryMethod, ICollection<OrderItem> items, decimal subtotal,string paymentIntentId)
		{
			BuyerEmail = buyerEmail;
			ShippingAddress = shippingAddress;
			DeliveryMethod = deliveryMethod;
			Items = items;
			Subtotal = subtotal;
			PaymentIntentId = paymentIntentId;
		}

		public string BuyerEmail { get; set; }
		public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;
		public OrderStatus Status { get; set; } = OrderStatus.Pending;
		public Address ShippingAddress { get; set; }
		//public int DeliveryMethodId { get; set; } //ForeignKey one
		public DeliveryMethod DeliveryMethod { get; set; } // Navigational Property one
		public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();

		public decimal Subtotal { get; set; } // Q*P
		public decimal GetTotal()
			=> Subtotal + DeliveryMethod.Cost;

		public string PaymentIntentId { get; set; }

	}
}
