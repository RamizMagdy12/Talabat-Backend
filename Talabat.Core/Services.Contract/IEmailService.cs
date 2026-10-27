namespace Talabat.Core.Services.Contract
{
	public interface IEmailService
	{
		Task SendAsync(string toEmail, string subject, string htmlBody);
	}
}
