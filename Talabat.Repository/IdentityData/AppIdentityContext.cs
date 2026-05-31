using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities.Identity;

namespace Talabat.Repository.IdentityData
{
   public class AppIdentityContext:IdentityDbContext<AppUser>
    {
		public AppIdentityContext(DbContextOptions<AppIdentityContext> options):base(options)
		{
			
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			builder.Entity<Address>().ToTable("Addreses");
		}
	}
}
