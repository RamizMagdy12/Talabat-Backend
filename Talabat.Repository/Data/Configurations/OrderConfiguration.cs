using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.Repository.Data.Configurations
{
	class OrderConfiguration : IEntityTypeConfiguration<Order>
	{
		public void Configure(EntityTypeBuilder<Order> builder)
		{
			builder.OwnsOne(o => o.ShippingAddress, ShippingAddress => ShippingAddress.WithOwner()); //owner => Order Table
			builder.Property(o => o.Status)
					.HasConversion(
					OStatus => OStatus.ToString(),
					OStatus => (OrderStatus)Enum.Parse(typeof(OrderStatus), OStatus)
				);


			builder.Property(o => o.Subtotal)
				.HasColumnType("decimal(18,2)");
					
		}
	}
}
