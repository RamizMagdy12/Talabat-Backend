using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Talabat.API.DTOs.AccountDto;
using Talabat.API.Errors;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Services.Contract;

namespace Talabat.API.Controllers
{
 
    public class AccountController : BaseAPIController
    {
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly ITokenService _tokenService;

		public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,ITokenService tokenService)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_tokenService = tokenService;
		}
		[HttpPost("login")]
		public async Task<ActionResult<UserDto>> Login(loginDto model)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
				return Unauthorized(new ApiResponse(401));

			var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

			if (result.Succeeded is false)
				return Unauthorized(new ApiResponse(401));

			return Ok(new UserDto()
			{
				DisplayName = user.DisplayName,
				Email = user.Email,
				Token = await _tokenService.CreateTokenAsync(user, _userManager)
			});
		}

		[HttpPost("register")]
		public async Task<ActionResult<UserDto>> Register(RegisterDto model)
		{
			// ✅ تحقق إن الإيميل مش موجود أصلاً
			if (await _userManager.FindByEmailAsync(model.Email) != null)
				return BadRequest(new ApiResponse(400, "Email already exists"));

			var user = new AppUser()
			{
				DisplayName = model.DisplayName,
				Email = model.Email,
				UserName = model.UserName,
				PhoneNumber = model.PhoneNumber,
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (result.Succeeded is false)
			{
				// ✅ رجّع الـ errors الحقيقية من Identity
				var errors = result.Errors.Select(e => e.Description).ToArray();
				return BadRequest(new { statusCode = 400, errors = errors });
			}

			return Ok(new UserDto()
			{
				DisplayName = user.DisplayName,
				Email = user.Email,
				Token = await _tokenService.CreateTokenAsync(user, _userManager)
			});
		}
		[HttpGet("emailexists")]
		public async Task<ActionResult<bool>> CheckEmailExists(string email)
		{
			return await _userManager.FindByEmailAsync(email) != null;
		}

	}
}
