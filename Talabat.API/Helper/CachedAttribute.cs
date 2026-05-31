using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Text;
using Talabat.Core.Services.Contract;

namespace Talabat.API.Helper
{
	public class CachedAttribute : Attribute, IAsyncActionFilter
	{
		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var responseCacheService = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();
			// Ask CLR for Creating object from "ResponseCacheService" Exciplictly

			var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);

			var response = await responseCacheService.GetCachedResponseAsync(cacheKey);

			if (!string.IsNullOrEmpty(response))
			{
				var contentResult = new ContentResult
				{
					Content = response,
					ContentType = "application/json",
					StatusCode = 200
				};
				context.Result = contentResult;
				return;
			}
			// If there is no cache, we will excute the action and get the result
			var executedContext = await next.Invoke();
			if(executedContext.Result is OkObjectResult okObjectResult && okObjectResult.Value is not null)
			{
				await responseCacheService.CacheResponseAsync(cacheKey, okObjectResult.Value, TimeSpan.FromSeconds(600));
			}
		}

		private static string GenerateCacheKeyFromRequest(HttpRequest request)
		{
			var keyBuilder = new StringBuilder();
			keyBuilder.Append(request.Path);

			var buyerEmail = request.HttpContext.User.FindFirstValue(ClaimTypes.Email);
			if (!string.IsNullOrWhiteSpace(buyerEmail))
				keyBuilder.Append($"|user-{buyerEmail.Trim()}");

			foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
			{
				keyBuilder.Append($"|{key}-{value}");
			}
			return keyBuilder.ToString();
		}
	}
}
