using System.ComponentModel.DataAnnotations;

namespace Talabat.Admin.Models
{
	public class CreateStaffViewModel
	{
		[Required, MaxLength(100)]
		[Display(Name = "Display name")]
		public string DisplayName { get; set; } = string.Empty;

		[Required, EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required, DataType(DataType.Password)]
		[MinLength(6)]
		public string Password { get; set; } = string.Empty;

		[Required, DataType(DataType.Password)]
		[Compare(nameof(Password))]
		[Display(Name = "Confirm password")]
		public string ConfirmPassword { get; set; } = string.Empty;

		/// <summary>If true, user gets full Admin role; permission checkboxes ignored.</summary>
		[Display(Name = "Make Admin (full access)")]
		public bool IsAdmin { get; set; }

		public List<string> SelectedPermissions { get; set; } = new();
	}

	public class EditStaffPermissionsViewModel
	{
		public string UserId { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string DisplayName { get; set; } = string.Empty;
		public bool IsAdmin { get; set; }
		public List<string> SelectedPermissions { get; set; } = new();
	}
}
