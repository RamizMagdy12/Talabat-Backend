namespace Talabat.API.DTOs.AccountDto
{
	public class ProfileDto
	{
		public string DisplayName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public string? PhoneNumber { get; set; }
		public ProfileAddressDto? Address { get; set; }
	}

	public class ProfileAddressDto
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Street { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string Country { get; set; } = string.Empty;
	}
}
