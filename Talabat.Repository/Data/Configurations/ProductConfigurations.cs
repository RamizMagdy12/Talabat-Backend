using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;

namespace Talabat.Repository.Data.Configurations
{
	public class ProductConfigurations : IEntityTypeConfiguration<Product>
	{
		public void Configure(EntityTypeBuilder<Product> builder)
		{
			builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
			builder.Property(p => p.NameAr).HasMaxLength(100);
			builder.Property(p => p.Description).IsRequired();
			builder.Property(p => p.DescriptionAr);
			builder.Property(p => p.PictureUrl).IsRequired();
			builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
			builder.Property(p => p.OfferPrice).HasColumnType("decimal(18,2)");
			builder.Property(p => p.Stock).HasDefaultValue(0);
			builder.Property(p => p.IsActive).HasDefaultValue(true);
			builder.HasOne(p => p.ProductBrand).WithMany().HasForeignKey(b => b.BrandId);
			builder.HasOne(p => p.productCategory).WithMany().HasForeignKey(c => c.categoryId);
		}
	}
}
