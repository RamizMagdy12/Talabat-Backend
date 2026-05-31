using System.ComponentModel.DataAnnotations;
using Talabat.API.Mappings;
using Talabat.Core.Entities;

namespace Talabat.API.DTOs
{
	public class BasketItemDto:IMapFrom<BasketItem>
	{
		[Required]
		public int Id { get; set; }
		[Required]
		public string ProductName { get; set; }
		[Required]
		public string ProductUrl { get; set; }
		[Required]
		[Range(0.1,double.MaxValue)]
		public decimal Price { get; set; }
		[Required]
		public string category { get; set; }
		[Required]
		public string Brand { get; set; }
		[Required]
		[Range(1,int.MaxValue)]
		public int Quantity { get; set; }
	}
}