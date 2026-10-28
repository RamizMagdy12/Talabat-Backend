using System.Security.Claims;

namespace Talabat.Admin.Security
{
	public static class AdminAccessExtensions
	{
		public static bool IsPanelAdmin(this ClaimsPrincipal? user) =>
			user?.IsInRole(AdminRoles.Admin) == true;

		public static bool IsPanelStaff(this ClaimsPrincipal? user) =>
			user?.IsInRole(AdminRoles.Staff) == true;

		public static bool CanAccessPanel(this ClaimsPrincipal? user) =>
			user?.IsPanelAdmin() == true || user?.IsPanelStaff() == true;

		public static bool HasModule(this ClaimsPrincipal? user, string permission)
		{
			if (user == null) return false;
			if (user.IsPanelAdmin()) return true;
			return user.IsPanelStaff()
				&& user.HasClaim(AdminPermissions.ClaimType, permission);
		}

		public static string HomeController(this ClaimsPrincipal user)
		{
			if (user.HasModule(AdminPermissions.Dashboard)) return "Dashboard";
			if (user.HasModule(AdminPermissions.Orders)) return "Orders";
			if (user.HasModule(AdminPermissions.Products)) return "Products";
			if (user.HasModule(AdminPermissions.Inventory)) return "Inventory";
			if (user.HasModule(AdminPermissions.Offers)) return "Offers";
			if (user.HasModule(AdminPermissions.Brands)) return "Brands";
			if (user.HasModule(AdminPermissions.Categories)) return "Categories";
			if (user.HasModule(AdminPermissions.Finance)) return "Finance";
			return "Profile";
		}
	}
}
