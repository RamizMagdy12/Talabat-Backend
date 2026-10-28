namespace Talabat.Admin.Security
{
	public static class AdminRoles
	{
		public const string Admin = "Admin";
		public const string Staff = "Staff";
	}

	public static class AdminPermissions
	{
		public const string ClaimType = "AdminPermission";

		public const string Dashboard = "Dashboard";
		public const string Products = "Products";
		public const string Inventory = "Inventory";
		public const string Brands = "Brands";
		public const string Categories = "Categories";
		public const string Orders = "Orders";
		public const string Offers = "Offers";
		public const string Finance = "Finance";

		public static readonly IReadOnlyList<(string Key, string LabelEn, string LabelAr)> All =
		[
			(Dashboard, "Dashboard", "لوحة التحكم"),
			(Products, "Products", "المنتجات"),
			(Inventory, "Inventory", "الجرد"),
			(Brands, "Brands", "العلامات"),
			(Categories, "Categories", "التصنيفات"),
			(Orders, "Orders", "الطلبات"),
			(Offers, "Offers", "العروض"),
			(Finance, "Finance", "المالية")
		];
	}
}
