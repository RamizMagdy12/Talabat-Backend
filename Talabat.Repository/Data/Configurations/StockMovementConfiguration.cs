using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabat.Core.Entities;

namespace Talabat.Repository.Data.Configurations
{
	public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
	{
		public void Configure(EntityTypeBuilder<StockMovement> builder)
		{
			builder.ToTable("StockMovements");
			builder.Property(m => m.Quantity).IsRequired();
			builder.Property(m => m.Note).HasMaxLength(500);
			builder.HasIndex(m => m.ProductId);
			builder.HasIndex(m => m.OccurredAtUtc);

			builder.HasOne(m => m.Product)
				.WithMany()
				.HasForeignKey(m => m.ProductId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
