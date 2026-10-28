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
		private readonly IConfiguration _configuration;
		private readonly ILogger<AccountController> _logger;

		public AccountController(
			UserManager<AppUser> userManager,
			SignInManager<AppUser> signInManager,
			ITokenService tokenService,
			IEmailService emailService,
			AppIdentityContext identityDb,
			IConfiguration configuration,
			ILogger<AccountController> logger)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_tokenService = tokenService;
			_emailService = emailService;
			_identityDb = identityDb;
			_configuration = configuration;
			_logger = logger;
		}

		[HttpPost("login")]
		public async Task<ActionResult<UserDto>> Login(loginDto model)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
				return Unauthorized(new ApiResponse(401));

			var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
			if (!result.Succeeded)
				return Unauthorized(new ApiResponse(401));

			if (!user.EmailConfirmed)
				return Unauthorized(new ApiResponse(401, "EmailNotConfirmed"));

			return Ok(new UserDto()
			{
				DisplayName = user.DisplayName,
				Email = user.Email,
				Token = await _tokenService.CreateTokenAsync(user, _userManager)
			});
		}

		[HttpPost("register")]
		public async Task<ActionResult> Register(RegisterDto model)
		{
			if (await _userManager.FindByEmailAsync(model.Email) != null)
				return BadRequest(new ApiResponse(400, "Email already exists"));

			var user = new AppUser()
			{
				DisplayName = model.DisplayName,
				Email = model.Email,
				UserName = model.UserName,
				PhoneNumber = model.PhoneNumber,
				EmailConfirmed = false
			};

			var result = await _userManager.CreateAsync(user, model.Password);
			if (!result.Succeeded)
			{
				var errors = result.Errors.Select(e => e.Description).ToArray();
				return BadRequest(new { statusCode = 400, errors });
			}

			try
			{
				await SendConfirmationEmailAsync(user);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
				return StatusCode(500, new ApiResponse(500, "Account created but confirmation email could not be sent. Use resend confirmation."));
			}

			return Ok(new
			{
				message = "Registration successful. Please check your email to activate your account."
			});
		}

		[HttpPost("confirmemail")]
		public async Task<ActionResult> ConfirmEmail(ConfirmEmailDto model)
		{
			if (string.IsNullOrWhiteSpace(model.UserId) || string.IsNullOrWhiteSpace(model.Token))
				return BadRequest(new ApiResponse(400, "Invalid confirmation link."));

			var user = await _userManager.FindByIdAsync(model.UserId);
			if (user == null)
				return BadRequest(new ApiResponse(400, "Invalid confirmation link."));

			if (user.EmailConfirmed)
				return Ok(new { message = "Email already confirmed. You can sign in." });

			var token = Uri.UnescapeDataString(model.Token);
			var result = await _userManager.ConfirmEmailAsync(user, token);
			if (!result.Succeeded)
			{
				var errors = result.Errors.Select(e => e.Description).ToArray();
				return BadRequest(new { statusCode = 400, message = "Could not confirm email.", errors });
			}

			return Ok(new { message = "Email confirmed. You can sign in now." });
		}

		[HttpPost("resendconfirmation")]
		public async Task<ActionResult> ResendConfirmation(ResendConfirmationDto model)
		{
			var genericOk = Ok(new
			{
				message = "If an account exists for this email and is not activated, a new confirmation link has been sent."
			});

			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null || string.IsNullOrWhiteSpace(user.Email) || user.EmailConfirmed)
				return genericOk;

			try
			{
				await SendConfirmationEmailAsync(user);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to resend confirmation email to {Email}", user.Email);
				return StatusCode(500, new ApiResponse(500, "Could not send the email. Check SMTP settings and try again."));
			}

			return genericOk;
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

		private async Task SendConfirmationEmailAsync(AppUser user)
		{
			if (string.IsNullOrWhiteSpace(user.Email))
				throw new InvalidOperationException("User has no email.");

			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var encodedToken = Uri.EscapeDataString(token);
			var clientAppUrl = (_configuration["ClientAppUrl"] ?? "http://localhost:4200").TrimEnd('/');
			var link = $"{clientAppUrl}/account/confirm-email?userId={Uri.EscapeDataString(user.Id)}&token={encodedToken}";
			var html = BuildConfirmEmailHtml(user.DisplayName ?? user.UserName ?? "there", link);
			await _emailService.SendAsync(user.Email, "Talabat — activate your account", html);
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

			for (var i = buffer.Length - 1; i > 0; i--)
			{
				var j = RandomNumberGenerator.GetInt32(i + 1);
				(buffer[i], buffer[j]) = (buffer[j], buffer[i]);
			}

			return new string(buffer);
		}

		private static string BuildConfirmEmailHtml(string name, string link)
		{
			var safeName = System.Net.WebUtility.HtmlEncode(name);
			var safeLink = System.Net.WebUtility.HtmlEncode(link);
			var sb = new StringBuilder();
			sb.Append("<div style='font-family:Segoe UI,Arial,sans-serif;max-width:560px;margin:0 auto;padding:24px;background:#f3f5f9;color:#14181f'>");
			sb.Append("<div style='background:#fff;border-radius:16px;padding:28px;border:1px solid #e4e8ef'>");
			sb.Append("<div style='width:42px;height:42px;border-radius:12px;background:linear-gradient(145deg,#ff4d00,#d93d00);color:#fff;font-weight:800;text-align:center;line-height:42px;font-size:20px'>T</div>");
			sb.Append("<h2 style='margin:16px 0 8px;font-size:22px'>Activate your account</h2>");
			sb.Append($"<p style='margin:0 0 12px;color:#7a8494'>Hi {safeName},</p>");
			sb.Append("<p style='margin:0 0 16px;color:#3d4654'>Thanks for joining Talabat. Confirm your email to activate your account:</p>");
			sb.Append($"<p style='margin:0 0 20px'><a href=\"{safeLink}\" style='display:inline-block;padding:12px 20px;background:#ff4d00;color:#fff;text-decoration:none;border-radius:10px;font-weight:700'>Activate account</a></p>");
			sb.Append($"<p style='margin:0 0 8px;color:#7a8494;font-size:13px;word-break:break-all'>Or open this link:<br/>{safeLink}</p>");
			sb.Append("<p style='margin:0;color:#7a8494;font-size:13px'>If you did not create this account, you can ignore this email.</p>");
			sb.Append("</div></div>");
			return sb.ToString();
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
