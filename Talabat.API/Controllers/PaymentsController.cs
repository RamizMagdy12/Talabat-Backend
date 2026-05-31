using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;
using Talabat.API.Errors;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Services.Contract;

namespace Talabat.API.Controllers
{

    public class PaymentsController : BaseAPIController
    {
		private readonly IPaymentService _paymentService;
		private readonly ILogger<PaymentsController> _logger;
		private readonly IConfiguration _configuration;
		public PaymentsController(IPaymentService paymentService,ILogger<PaymentsController>logger,IConfiguration configuration)
		{
			_paymentService = paymentService;
			_logger = logger;
			_configuration = configuration;
		}
		[Authorize]
		[HttpPost("{basketId}")]
		public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePayment(string basketId)
		{
			var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
			if (string.IsNullOrWhiteSpace(buyerEmail))
				return Unauthorized(new ApiResponse(401, "Buyer email is required"));

			var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId, buyerEmail);
			if (basket == null) return BadRequest(new ApiResponse(400, "Problem with your basket"));
			return basket;
		}
		[AllowAnonymous]
		[HttpPost("webhook")]
		public async Task<IActionResult> StripeWebhook()
		{
			var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

			Event stripeEvent;
			try
			{
				stripeEvent = EventUtility.ConstructEvent(json,
					Request.Headers["Stripe-Signature"], _configuration["StripeSettings:WebhookSecret"]);
			}
			catch (StripeException ex)
			{
				_logger.LogError(ex, "Invalid Stripe webhook signature or payload");
				return BadRequest();
			}

			switch (stripeEvent.Type)
			{
				case EventTypes.PaymentIntentSucceeded:
					{
						var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
						var order = await WaitForOrderAsync(paymentIntent!.Id);

						if (order is null)
							_logger.LogWarning("Order not found after retries: {Id}", paymentIntent.Id);
						else
							_logger.LogInformation("Payment Succeeded: {Id}", paymentIntent.Id);
						break;
					}

				case EventTypes.PaymentIntentPaymentFailed:
					{
						var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
						var order = await WaitForOrderAsync(paymentIntent!.Id, isSucceeded: false);

						if (order is null)
							_logger.LogWarning("Order not found after retries: {Id}", paymentIntent.Id);
						else
							_logger.LogInformation("Payment Failed: {Id}", paymentIntent.Id);
						break;
					}
			}

			return Ok();
			
			

		}

		private async Task<Order?> WaitForOrderAsync(
	string paymentIntentId,
	bool isSucceeded = true,
	int maxRetries = 5,
	int delayMs = 2000)
		{
			for (int i = 0; i < maxRetries; i++)
			{
				var order = await _paymentService.UpdatePaymentIntentToSucceedOrFaild(paymentIntentId, isSucceeded);

				if (order is not null)
					return order;

				_logger.LogWarning("Retry {Attempt}/{Max} for PaymentIntent: {Id}", i + 1, maxRetries, paymentIntentId);
				await Task.Delay(delayMs);
			}
			return null;
		}
	}
}
