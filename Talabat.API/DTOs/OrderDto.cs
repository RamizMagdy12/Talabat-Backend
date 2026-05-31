using System.ComponentModel.DataAnnotations;

namespace Talabat.API.DTOs
{
	public class OrderDto
	{
		//[Required]
		//public string BuyerEmail { get; set; }
		[Required]
		public string BasketId { get; set; }
		[Required]
		public int DeliveryMethod { get; set; }
		[Required]
		public AddressDto ShippingAddress { get; set; }
	}
}
