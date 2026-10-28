using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Talabat.Admin.Security
{
	/// <summary>
	/// Allows Admins always. Staff users need at least one of the listed permission claims.
	/// With no permissions listed, any Admin or Staff user may access.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public sealed class AdminPermissionAttribute : Attribute, IAuthorizationFilter
	{
		private readonly string[] _permissions;

		public AdminPermissionAttribute(params string[] permissions)
		{
			_permissions = permissions ?? Array.Empty<string>();
		}

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			var user = context.HttpContext.User;
			if (user?.Identity?.IsAuthenticated != true)
			{
				context.Result = new ChallengeResult();
				return;
			}

			if (user.IsInRole(AdminRoles.Admin))
				return;

			if (!user.IsInRole(AdminRoles.Staff))
			{
				context.Result = new ForbidResult();
				return;
			}

			if (_permissions.Length == 0)
				return;

			var allowed = _permissions.Any(p =>
				user.HasClaim(AdminPermissions.ClaimType, p));

			if (!allowed)
				context.Result = new ForbidResult();
		}
	}
}
