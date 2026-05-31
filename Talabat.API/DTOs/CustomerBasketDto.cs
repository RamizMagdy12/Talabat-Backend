using System.ComponentModel.DataAnnotations;
using Talabat.API.Mappings;
using Talabat.Core.Entities;

namespace Talabat.API.DTOs
{
	// because validation
	public class CustomerBasketDto:IMapFrom<CustomerBasket>
	{
		[Required]
		public string Id { get; set; }
		public List<BasketItemDto> Items { get; set; } 
	}
}
