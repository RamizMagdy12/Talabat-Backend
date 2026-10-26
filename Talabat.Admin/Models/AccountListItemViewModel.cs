namespace Talabat.Admin.Models
{
	public class AccountListItemViewModel
	{
		public string Id { get; set; } = string.Empty;
		public string DisplayName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public string? PhoneNumber { get; set; }
		public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
		public int OrdersToday { get; set; }
		public int OrdersThisMonth { get; set; }
		public int TotalOrders { get; set; }
	}
}
