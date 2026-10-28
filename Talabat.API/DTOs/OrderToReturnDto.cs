using AutoMapper;
using Talabat.API.Mappings;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.API.DTOs
{
	public class OrderToReturnDto:IMapFrom<Order>
	{
		public int Id { get; set; }
		public string OrderNumber { get; set; } = string.Empty;
		public string BuyerEmail { get; set; }
		public DateTimeOffset OrderDate { get; set; } 
		public OrderStatus Status { get; set; } 
		public Address ShippingAddress { get; set; }
		
		public string DeliveryMethod { get; set; }
		public decimal DeliveryMethodCost { get; set; }
		public ICollection<OrderItemDto> Items { get; set; } = new HashSet<OrderItemDto>();

		public decimal Subtotal { get; set; } 
		public decimal Total { get; set; }
		public string PaymentIntentId { get; set; } = string.Empty;

		public void Mapping(Profile profile)
		{
			profile.CreateMap<Order, OrderToReturnDto>()
				.ForMember(d => d.DeliveryMethod,
					o => o.MapFrom(s => s.DeliveryMethod.ShortName))
				.ForMember(d => d.DeliveryMethodCost,
					o => o.MapFrom(s => s.DeliveryMethod.Cost))
				.ForMember(d => d.Total,
					o => o.MapFrom(s => s.GetTotal()));
		}
	}
}
