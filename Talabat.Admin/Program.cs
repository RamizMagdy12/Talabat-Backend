using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Talabat.Admin.Localization;
using Talabat.Core;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Services.Contract;
using Talabat.Repository;
using Talabat.Repository.Data;
using Talabat.Repository.IdentityData;
using Talabat.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<SharedLocalizer>();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
	// Culture (numbers/currency/dates) stays en-US; UI culture switches en/ar for strings.
	options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en");
	options.SupportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("ar") };
	options.SupportedUICultures = new[] { new CultureInfo("en"), new CultureInfo("ar") };
	options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
});

builder.Services.AddDbContext<StoreContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddDbContext<AppIdentityContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
	ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireUppercase = true;
	options.Password.RequireNonAlphanumeric = true;
	options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppIdentityContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/Account/Login";
	options.AccessDeniedPath = "/Account/AccessDenied";
	options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<IResponseCacheService, ResponseCacheService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		var storeContext = services.GetRequiredService<StoreContext>();
		await storeContext.Database.MigrateAsync();
		await StoreContextSeed.SeedAsync(storeContext);

		var identityContext = services.GetRequiredService<AppIdentityContext>();
		await identityContext.Database.MigrateAsync();

		var userManager = services.GetRequiredService<UserManager<AppUser>>();
		var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
		await AdminIdentitySeed.SeedAsync(userManager, roleManager);
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
		logger.LogError(ex, "Admin startup migration/seed failed");
	}
}

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public static class AdminIdentitySeed
{
	public static async Task SeedAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
	{
		foreach (var role in new[] { "Admin", "Staff" })
		{
			if (!await roleManager.RoleExistsAsync(role))
				await roleManager.CreateAsync(new IdentityRole(role));
		}

		const string adminEmail = "admin@talabat.local";
		var admin = await userManager.FindByEmailAsync(adminEmail);
		if (admin == null)
		{
			admin = new AppUser
			{
				DisplayName = "Talabat Admin",
				Email = adminEmail,
				UserName = "talabat.admin",
				EmailConfirmed = true
			};
			var result = await userManager.CreateAsync(admin, "Admin@123");
			if (!result.Succeeded) return;
		}

		if (!await userManager.IsInRoleAsync(admin, "Admin"))
			await userManager.AddToRoleAsync(admin, "Admin");
	}
}
