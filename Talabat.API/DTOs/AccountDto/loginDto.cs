using System.ComponentModel.DataAnnotations;

namespace Talabat.API.DTOs.AccountDto
{
	public class loginDto
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
