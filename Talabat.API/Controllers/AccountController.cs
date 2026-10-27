using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Talabat.API.DTOs.AccountDto;
using Talabat.API.Errors;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Services.Contract;
using Talabat.Repository.IdentityData;

namespace Talabat.API.Controllers
{
 
    public class AccountController : BaseAPIController
    {
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly ITokenService _tokenService;
		private readonly IEmailService _emailService;
		private readonly AppIdentityContext _identityDb;

		public AccountController(
			UserManager<AppUser> userManager,
			SignInManager<AppUser> signInManager,
			ITokenService tokenService,
			IEmailService emailService,
			AppIdentityContext identityDb)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_tokenService = tokenService;
			_emailService = emailService;
			_identityDb = identityDb;
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

		[Authorize]
		[HttpGet("profile")]
		public async Task<ActionResult<ProfileDto>> GetCurrentUser()
		{
			var user = await GetUserWithAddressAsync();
			if (user == null) return Unauthorized(new ApiResponse(401));
			return Ok(MapProfile(user));
		}

		[Authorize]
		[HttpPut("profile")]
		[HttpPost("profile")]
		public async Task<ActionResult<ProfileDto>> UpdateProfile(UpdateProfileDto model)
		{
			var user = await GetUserWithAddressAsync();
			if (user == null) return Unauthorized(new ApiResponse(401));

			user.DisplayName = model.DisplayName.Trim();
			user.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();

			if (model.Address != null)
			{
				if (user.Address == null)
				{
					_identityDb.Set<Address>().Add(new Address
					{
						AppUserId = user.Id,
						FName = model.Address.FirstName?.Trim() ?? string.Empty,
						LName = model.Address.LastName?.Trim() ?? string.Empty,
						Street = model.Address.Street?.Trim() ?? string.Empty,
						City = model.Address.City?.Trim() ?? string.Empty,
						Country = model.Address.Country?.Trim() ?? string.Empty
					});
				}
				else
				{
					user.Address.FName = model.Address.FirstName?.Trim() ?? string.Empty;
					user.Address.LName = model.Address.LastName?.Trim() ?? string.Empty;
					user.Address.Street = model.Address.Street?.Trim() ?? string.Empty;
					user.Address.City = model.Address.City?.Trim() ?? string.Empty;
					user.Address.Country = model.Address.Country?.Trim() ?? string.Empty;
				}
			}

			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				var errors = result.Errors.Select(e => e.Description).ToArray();
				return BadRequest(new { statusCode = 400, errors });
			}

			await _identityDb.SaveChangesAsync();
			user = await GetUserWithAddressAsync() ?? user;
			return Ok(MapProfile(user));
		}

		[Authorize]
		[HttpPost("changepassword")]
		public async Task<ActionResult> ChangePassword(ChangePasswordDto model)
		{
			var user = await GetUserWithAddressAsync();
			if (user == null) return Unauthorized(new ApiResponse(401));

			var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
			if (!result.Succeeded)
			{
				var errors = result.Errors.Select(e => e.Description).ToArray();
				return BadRequest(new { statusCode = 400, message = errors.FirstOrDefault() ?? "Could not change password.", errors });
			}

			return Ok(new { message = "Password changed successfully." });
		}

		private async Task<AppUser?> GetUserWithAddressAsync()
		{
			var email = User.FindFirstValue(ClaimTypes.Email);
			if (string.IsNullOrWhiteSpace(email)) return null;

			return await _userManager.Users
				.Include(u => u.Address)
				.FirstOrDefaultAsync(u => u.Email == email);
		}

		private static ProfileDto MapProfile(AppUser user) => new()
		{
			DisplayName = user.DisplayName ?? string.Empty,
			Email = user.Email ?? string.Empty,
			UserName = user.UserName ?? string.Empty,
			PhoneNumber = user.PhoneNumber,
			Address = user.Address == null ? null : new ProfileAddressDto
			{
				FirstName = user.Address.FName ?? string.Empty,
				LastName = user.Address.LName ?? string.Empty,
				Street = user.Address.Street ?? string.Empty,
				City = user.Address.City ?? string.Empty,
				Country = user.Address.Country ?? string.Empty
			}
		};

		/// <summary>
		/// Generates a new password, emails it via Gmail SMTP, then applies the reset.
		/// Always returns a generic success when the email is unknown (no account enumeration).
		/// </summary>
		[HttpPost("forgotpassword")]
		public async Task<ActionResult> ForgotPassword(ForgotPasswordDto model)
		{
			var genericOk = Ok(new
			{
				message = "If an account exists for this email, a new password has been sent."
			});

			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null || string.IsNullOrWhiteSpace(user.Email))
				return genericOk;

			var newPassword = GenerateTemporaryPassword();
			var html = BuildResetEmailHtml(user.DisplayName ?? user.UserName ?? "there", newPassword);

			try
			{
				await _emailService.SendAsync(user.Email, "Talabat — your new password", html);
			}
			catch
			{
				return StatusCode(500, new ApiResponse(500, "Could not send the email. Check Gmail SMTP settings and try again."));
			}

			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			var reset = await _userManager.ResetPasswordAsync(user, token, newPassword);
			if (!reset.Succeeded)
			{
				var errors = reset.Errors.Select(e => e.Description).ToArray();
				return BadRequest(new { statusCode = 400, errors });
			}

			return genericOk;
		}

		private static string GenerateTemporaryPassword(int length = 12)
		{
			const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
			const string lower = "abcdefghijkmnopqrstuvwxyz";
			const string digits = "23456789";
			const string symbols = "!@#$%&*?";
			var all = upper + lower + digits + symbols;

			Span<char> buffer = stackalloc char[length];
			buffer[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
			buffer[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
			buffer[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
			buffer[3] = symbols[RandomNumberGenerator.GetInt32(symbols.Length)];
			for (var i = 4; i < length; i++)
				buffer[i] = all[RandomNumberGenerator.GetInt32(all.Length)];

			// shuffle
			for (var i = buffer.Length - 1; i > 0; i--)
			{
				var j = RandomNumberGenerator.GetInt32(i + 1);
				(buffer[i], buffer[j]) = (buffer[j], buffer[i]);
			}

			return new string(buffer);
		}

		private static string BuildResetEmailHtml(string name, string password)
		{
			var sb = new StringBuilder();
			sb.Append("<div style='font-family:Segoe UI,Arial,sans-serif;max-width:560px;margin:0 auto;padding:24px;background:#f3f5f9;color:#14181f'>");
			sb.Append("<div style='background:#fff;border-radius:16px;padding:28px;border:1px solid #e4e8ef'>");
			sb.Append("<div style='width:42px;height:42px;border-radius:12px;background:linear-gradient(145deg,#ff4d00,#d93d00);color:#fff;font-weight:800;text-align:center;line-height:42px;font-size:20px'>T</div>");
			sb.Append("<h2 style='margin:16px 0 8px;font-size:22px'>Password reset</h2>");
			sb.Append($"<p style='margin:0 0 12px;color:#7a8494'>Hi {System.Net.WebUtility.HtmlEncode(name)},</p>");
			sb.Append("<p style='margin:0 0 16px;color:#3d4654'>We generated a new password for your Talabat account:</p>");
			sb.Append($"<p style='margin:0 0 20px;padding:14px 16px;background:#fff1ea;border:1px solid #ffd2bf;border-radius:12px;font-size:20px;font-weight:800;letter-spacing:0.04em;color:#d93d00'>{System.Net.WebUtility.HtmlEncode(password)}</p>");
			sb.Append("<p style='margin:0 0 8px;color:#7a8494;font-size:14px'>Sign in with this password, then change it from your account if you want.</p>");
			sb.Append("<p style='margin:0;color:#7a8494;font-size:13px'>If you did not request this, contact support right away.</p>");
			sb.Append("</div></div>");
			return sb.ToString();
		}
	}
}
