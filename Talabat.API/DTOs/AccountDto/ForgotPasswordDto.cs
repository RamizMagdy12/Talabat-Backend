using System.ComponentModel.DataAnnotations;

namespace Talabat.API.DTOs.AccountDto
{
	public class ForgotPasswordDto
	{
		[Required, EmailAddress]
		public string Email { get; set; } = string.Empty;
	}
}
