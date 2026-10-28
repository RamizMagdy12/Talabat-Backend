using System.ComponentModel.DataAnnotations;

namespace Talabat.API.DTOs.AccountDto
{
	public class ConfirmEmailDto
	{
		[Required]
		public string UserId { get; set; } = string.Empty;

		[Required]
		public string Token { get; set; } = string.Empty;
	}
}
