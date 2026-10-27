using System.ComponentModel.DataAnnotations;
using Talabat.API.Mappings;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.API.DTOs
{
	public class AddressDto:IMapFrom<Address>
	{
		[Required]
		public string FirstName { get; set; }
		[Required]
		public string LastName { get; set; }
		[Required]
		public string Street { get; set; }
		[Required]
		public string City { get; set; }
		[Required]
		public string Country { get; set; }
		[MaxLength(30)]
		public string PhoneNumber { get; set; } = string.Empty;
	}
}