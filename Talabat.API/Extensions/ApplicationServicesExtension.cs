using Talabat.API.Mappings;
using Talabat.Core;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Services.Contract;
using Talabat.Repository;
using Talabat.Services;

namespace Talabat.API.Extensions
{
	public static class ApplicationServicesExtension
	{
		public static IServiceCollection AddAppServices(this IServiceCollection services) {
			services.AddSingleton<IResponseCacheService, ResponseCacheService>();
			services.AddScoped(typeof(IPaymentService),typeof(PaymentService));
			services.AddScoped(typeof(IUnitOfWork),typeof(UnitOfWork));
			//services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
			services.AddScoped<IBasketRepository, BasketRepository>();
			services.AddScoped<ITokenService, TokenService>();
			services.AddScoped(typeof(IOrderService), typeof(OrderService));
			services.AddScoped<IProductService, ProductService>();
			services.AddAutoMapper(typeof(MappingProfile));

			return services;
		}
	}
}
