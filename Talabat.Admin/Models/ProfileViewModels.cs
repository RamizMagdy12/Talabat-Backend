using System.ComponentModel.DataAnnotations;

namespace Talabat.Admin.Models
{
	public class ProfileViewModel
	{
		public string Email { get; set; } = string.Empty;
		public string DisplayName { get; set; } = string.Empty;
		public string RoleLabel { get; set; } = string.Empty;
		public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
	}

	public class ChangePasswordViewModel
	{
		[Required, DataType(DataType.Password)]
		[Display(Name = "Current password")]
		public string CurrentPassword { get; set; } = string.Empty;

		[Required, DataType(DataType.Password)]
		[MinLength(6)]
		[Display(Name = "New password")]
		public string NewPassword { get; set; } = string.Empty;

		[Required, DataType(DataType.Password)]
		[Compare(nameof(NewPassword))]
		[Display(Name = "Confirm new password")]
		public string ConfirmPassword { get; set; } = string.Empty;
	}
}