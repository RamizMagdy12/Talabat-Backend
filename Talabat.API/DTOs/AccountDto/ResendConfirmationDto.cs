using System.ComponentModel.DataAnnotations;

namespace Talabat.API.DTOs.AccountDto
{
	public class ResendConfirmationDto
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;
	}
}
