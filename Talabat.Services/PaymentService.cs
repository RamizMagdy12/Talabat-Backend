using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Services.Contract;
using Talabat.Core.Specifications.order_spec;

namespace Talabat.Services
{
	public class PaymentService : IPaymentService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IBasketRepository _basketRepository;
		private readonly IConfiguration _configuration;
		private readonly ILogger<PaymentService> _logger;

		public PaymentService(IUnitOfWork unitOfWork,IBasketRepository basketRepository,IConfiguration configuration,ILogger<PaymentService> logger)
		{
			_unitOfWork = unitOfWork;
			_basketRepository = basketRepository;
			_configuration = configuration;
			_logger = logger;
		}
		public async Task<CustomerBasket?> CreateOrUpdatePaymentIntent(string basketId, string buyerEmail)
		{
			if (string.IsNullOrWhiteSpace(buyerEmail))
			{
				_logger.LogWarning("Payment intent creation skipped: buyer email is missing");
				return null;
			}

			ConfigureStripe();

			var basket = await _basketRepository.GetBasketAsync(basketId);

			if (basket == null) return null;

			var stripeCustomerId = await GetOrCreateStripeCustomerIdAsync(buyerEmail);
			var ShippingPrice = 0m;
			if (basket.DeliveryMethodId.HasValue)
			{
				var deliveryMethod = await _unitOfWork.Repository<Core.Entities.Order_Aggregate.DeliveryMethod>().GetAsync(basket.DeliveryMethodId.Value);
				ShippingPrice = deliveryMethod.Cost;
			}

			if (basket?.Items?.Count > 0) {
				foreach (var item in basket.Items)
				{
					var product = await _unitOfWork.Repository<Core.Entities.Product>().GetAsync(item.Id);
					if (item.Price != product?.Price) {
						item.Price = product.Price;
					}
				}
			}
			var amount = (long)basket.Items.Sum(i => i.Quantity * i.Price * 100) + (long)ShippingPrice * 100;
			var paymentIntentService = new PaymentIntentService();
			PaymentIntent paymentIntent;

			if (string.IsNullOrEmpty(basket.PaymentIntentId))
			{
				var option = new PaymentIntentCreateOptions()
				{
					Amount = amount,
					Currency = "usd",
					Customer = stripeCustomerId,
					PaymentMethodTypes = new List<string>() { "card" }
				};
				paymentIntent = await paymentIntentService.CreateAsync(option);
				basket.PaymentIntentId = paymentIntent.Id;
				basket.ClientSecret = paymentIntent.ClientSecret;
			}
			else
			{
				var option = new PaymentIntentUpdateOptions()
				{
					Amount = amount,
					Customer = stripeCustomerId
				};
				paymentIntent = await paymentIntentService.UpdateAsync(basket.PaymentIntentId, option);
				basket.ClientSecret = paymentIntent.ClientSecret;
			}
			await _basketRepository.UpdateBasketAsync(basket);
			return basket;
		}

		private void ConfigureStripe()
			=> StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];

		private async Task<string> GetOrCreateStripeCustomerIdAsync(string buyerEmail)
		{
			var customerService = new CustomerService();

			var existingCustomers = await customerService.ListAsync(new CustomerListOptions
			{
				Email = buyerEmail,
				Limit = 1
			});

			if (existingCustomers.Data.Count > 0)
				return existingCustomers.Data[0].Id;

			var customer = await customerService.CreateAsync(new CustomerCreateOptions
			{
				Email = buyerEmail
			});

			_logger.LogInformation("Created Stripe customer {CustomerId} for {Email}", customer.Id, buyerEmail);
			return customer.Id;
		}

		public async Task<bool> IsPaymentIntentSucceededAsync(string paymentIntentId)
		{
			if (string.IsNullOrWhiteSpace(paymentIntentId))
				return false;

			ConfigureStripe();
			var service = new PaymentIntentService();
			var paymentIntent = await service.GetAsync(paymentIntentId);
			return paymentIntent.Status == "succeeded";
		}

		public async Task<Order?> UpdatePaymentIntentToSucceedOrFaild(string paymentIntentId, bool isSucceeded)
		{
			var spec = new OrderWithPaymentIntentSpecifications(paymentIntentId);
			var order = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);

			if (order is null)
			{
				_logger.LogWarning("Order not found for PaymentIntent: {PaymentIntentId}", paymentIntentId);
				return null; // ✅ مش هيكرش
			}

			var newStatus = isSucceeded ? OrderStatus.PaymentReceived : OrderStatus.PaymentFailed;
			if (order.Status == newStatus)
				return order;

			order.Status = newStatus;
			_unitOfWork.Repository<Order>().Update(order);
			await _unitOfWork.CompleteAsync();
			return order;
		}
	}
}
