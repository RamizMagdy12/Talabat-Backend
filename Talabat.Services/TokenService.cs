using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Services.Contract;

namespace Talabat.Services
{
	public class TokenService : ITokenService
	{
		private readonly IConfiguration _configuration;

		public TokenService(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		public async Task<string> CreateTokenAsync(AppUser user, UserManager<AppUser> userManager)
		{

			// private claims ()
			var authClaims = new List<Claim>()
			{
				new Claim(ClaimTypes.GivenName, user.DisplayName),
				new Claim(ClaimTypes.Email, user.Email)
			};
			var userRoles = await userManager.GetRolesAsync(user);
			foreach (var role in userRoles) 
				authClaims.Add(new Claim(ClaimTypes.Role, role));

			var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:key"]));

			var token = new JwtSecurityToken(
				issuer: _configuration["JWT:validIssure"],
				audience: _configuration["JWT:validAudience"],
				expires: DateTime.UtcNow.AddDays(double.Parse(_configuration["JWT:DurationInDays"])),
				claims: authClaims,
				signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256Signature)



				);

				  return new JwtSecurityTokenHandler().WriteToken(token);


		}
	}
}
