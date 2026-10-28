
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using System.Threading.Tasks;
using Talabat.API.Errors;
using Talabat.API.Extensions;
using Talabat.API.Mappings;
using Talabat.API.Middlewares;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Repositories.Contract;
using Talabat.Repository;
using Talabat.Repository.Data;
using Talabat.Repository.IdentityData;
using Talabat.Repository.IdentityData.DataSeeding;

namespace Talabat.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<StoreContext>(options =>
            {
                options.UseSqlServer(
             builder.Configuration.GetConnectionString("DefaultConnection"),
             sqlOptions => sqlOptions.EnableRetryOnFailure());


			});
            builder.Services.AddDbContext<AppIdentityContext>(options =>
            {
				options.UseSqlServer(
	 builder.Configuration.GetConnectionString("IdentityConnection"),
	 sqlOptions => sqlOptions.EnableRetryOnFailure()
 );
			});
            // Redis
            builder.Services.AddSingleton<IConnectionMultiplexer>((serviceProvider) =>
            {
                var connection = builder.Configuration.GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(connection);
            });

            builder.Services.AddIdentity<AppUser, IdentityRole>()
				.AddEntityFrameworkStores<AppIdentityContext>()
				.AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = builder.Configuration["JWT:validIssure"],
					ValidAudience = builder.Configuration["JWT:validAudience"],
					IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:key"])),
					ClockSkew = TimeSpan.Zero
				};
			});

			ApplicationServicesExtension.AddAppServices(builder.Services);
            // Add services to the container.
            //builder.Services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));

            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            //builder.Services.AddAutoMapper(typeof(MappingProfile));

            // Customize the validation error response [???? ?????? ????????? ???? ?????? ??????? ?????]
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage).ToArray();
                    var errorResponse = new ApiValidationErrorResponse
                    {
                        Errors = errors
                    };
                    return new BadRequestObjectResult(errorResponse);
                };
            });
            var app = builder.Build();
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var storecontext = services.GetRequiredService<StoreContext>();
			var _identityContext = services.GetRequiredService<AppIdentityContext>();
			var LoggerFActory = services.GetRequiredService<ILoggerFactory>();

            try
            {
                await storecontext.Database.MigrateAsync();
                await StoreContextSeed.SeedAsync(storecontext);
                await _identityContext.Database.MigrateAsync();
                var _userManager = services.GetRequiredService<UserManager<AppUser>>();
                await AppIdentityContextSeed.SeedUserAsync(_userManager);
				await ConfirmLegacyEmailsOnceAsync(app, _identityContext);
            }
            catch (Exception ex)
            {

                var logger = LoggerFActory.CreateLogger<Program>();
                logger.LogError(ex, ex.Message);
            }

			// Configure the HTTP request pipeline.
			app.UseMiddleware<ExceptionMiddleware>();
			if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
           app.UseStatusCodePagesWithRedirects("/errors/{0}");  
			app.UseHttpsRedirection();

            app.UseCors("AllowAngularApp");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }

		/// <summary>
		/// One-time: mark pre-existing Identity users as email-confirmed so login keeps working
		/// after email activation is enabled. Skips on subsequent startups via a marker file.
		/// </summary>
		private static async Task ConfirmLegacyEmailsOnceAsync(WebApplication app, AppIdentityContext identityContext)
		{
			var markerPath = Path.Combine(app.Environment.ContentRootPath, ".email-activation-backfill");
			if (File.Exists(markerPath))
				return;

			await identityContext.Database.ExecuteSqlRawAsync(
				"UPDATE AspNetUsers SET EmailConfirmed = 1 WHERE EmailConfirmed = 0");
			await File.WriteAllTextAsync(markerPath, DateTime.UtcNow.ToString("O"));
		}
    }
}
