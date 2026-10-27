using System.ComponentModel.DataAnnotations;

namespace Talabat.API.DTOs.AccountDto
{
	public class UpdateProfileDto
	{
		[Required, MaxLength(100)]
		public string DisplayName { get; set; } = string.Empty;

		[MaxLength(30)]
		public string? PhoneNumber { get; set; }

		public ProfileAddressDto? Address { get; set; }
	}
}
