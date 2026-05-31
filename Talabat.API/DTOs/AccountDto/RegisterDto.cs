using System.ComponentModel.DataAnnotations;

namespace Talabat.API.DTOs.AccountDto
{
	public class RegisterDto
	{
		[Required]
		public string DisplayName { get; set; }
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		public string UserName { get; set; }
		public string PhoneNumber { get; set; }
		[Required]

		public string Password { get; set; }
	}
}
