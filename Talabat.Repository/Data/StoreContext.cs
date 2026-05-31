using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;


namespace Talabat.Repository.Data
{
   public class StoreContext:DbContext
    {
		public StoreContext(DbContextOptions<StoreContext> options):base(options)
		{
			
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
			foreach(var entityType in modelBuilder.Model.GetEntityTypes())
			{
				if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
				{
					var parameter = Expression.Parameter(entityType.ClrType, "e");
					var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
					var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(false)), parameter);

					modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
				}
			}
		}

		public DbSet<Product> products { get; set; }
		public DbSet<ProductBrand> productBrands { get; set; }
		public DbSet<ProductCategory> productCategories { get; set; }
		public DbSet<Order> orders { get; set; }
		public DbSet<DeliveryMethod> deliveryMethods { get; set; }
		public DbSet<OrderItem> orderItem { get; set; }
	}
}
