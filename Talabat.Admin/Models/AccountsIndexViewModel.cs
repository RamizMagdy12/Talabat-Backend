namespace Talabat.Admin.Models
{
	public class AccountsIndexViewModel
	{
		public string? Search { get; set; }
		public int TotalAccounts { get; set; }
		public int OrdersToday { get; set; }
		public int OrdersThisMonth { get; set; }
		public IReadOnlyList<AccountListItemViewModel> Accounts { get; set; } = Array.Empty<AccountListItemViewModel>();
	}
}
