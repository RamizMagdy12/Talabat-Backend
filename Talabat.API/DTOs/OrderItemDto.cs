using AutoMapper;
using Talabat.API.Helper;
using Talabat.API.Mappings;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.API.DTOs
{
	public class OrderItemDto:IMapFrom<OrderItem>
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public string PictureUrl { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }

		public void Mapping(Profile profile)
		{
			profile.CreateMap<OrderItem, OrderItemDto>()
				.ForMember(d => d.ProductId,
					o => o.MapFrom(s => s.Product.ProductId))
				.ForMember(d => d.ProductName,
					o => o.MapFrom(s => s.Product.ProductName))
				.ForMember(d => d.PictureUrl,
					o => o.MapFrom(s => s.Product.ProductUrl))
				.ForMember(d => d.PictureUrl, o => o.MapFrom<OrderItemPictureUrlResolver>());
		}

	}
}