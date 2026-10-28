using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities.Identity;

namespace Talabat.Repository.IdentityData.DataSeeding
{
   public class AppIdentityContextSeed
    {
        public static async Task SeedUserAsync(UserManager<AppUser> _userManager)
        {
            if (_userManager.Users.Count() == 0)
            {
                var user = new AppUser()
                {
                    DisplayName = "Ramiz Magdy",
                    Email = "Ramiz@gmail.com",
                    UserName = "Ramiz.magdy",
                    PhoneNumber = "01226725667",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(user, "A@a123456");
            }
        }
    }
}
