using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Talabat.Core.Services.Contract;

namespace Talabat.Services
{
	public class EmailService : IEmailService
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<EmailService> _logger;

		public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
		{
			_configuration = configuration;
			_logger = logger;
		}

		public async Task SendAsync(string toEmail, string subject, string htmlBody)
		{
			var host = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
			var port = int.TryParse(_configuration["EmailSettings:SmtpPort"], out var p) ? p : 587;
			var fromEmail = _configuration["EmailSettings:FromEmail"]
				?? throw new InvalidOperationException("EmailSettings:FromEmail is missing.");
			var fromName = _configuration["EmailSettings:FromName"] ?? "Talabat Store";
			var password = _configuration["EmailSettings:AppPassword"]
				?? throw new InvalidOperationException("EmailSettings:AppPassword is missing. Use a Gmail App Password.");

			using var message = new MailMessage
			{
				From = new MailAddress(fromEmail, fromName),
				Subject = subject,
				Body = htmlBody,
				IsBodyHtml = true
			};
			message.To.Add(toEmail);

			using var client = new SmtpClient(host, port)
			{
				EnableSsl = true,
				Credentials = new NetworkCredential(fromEmail, password),
				DeliveryMethod = SmtpDeliveryMethod.Network
			};

			try
			{
				await client.SendMailAsync(message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send email to {Email}", toEmail);
				throw;
			}
		}
	}
}
