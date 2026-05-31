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
	public class OrderService : IOrderService
	{
		private readonly IBasketRepository _basketRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPaymentService _paymentService;

		///private readonly IGenericRepository<Product> _productRepo;
		///private readonly IGenericRepository<DeliveryMethod> _deliveryMethodRepo;
		///private readonly IGenericRepository<Order> _orderRepo;

		public OrderService(IBasketRepository basketRepository,IUnitOfWork unitOfWork,IPaymentService paymentService)
		{
			_basketRepository = basketRepository;
			_unitOfWork = unitOfWork;
			_paymentService = paymentService;
			///_productRepo = productRepo;
			///_deliveryMethodRepo = deliveryMethodRepo;
			///_orderRepo = OrderRepo;
		}
		public async Task<Order?> CreateOrderAsync(string BuyerEmail, string basketId, int deliveryMethodId, Address ShippingAddress)
		{
			if (string.IsNullOrWhiteSpace(BuyerEmail))
				return null;

			BuyerEmail = BuyerEmail.Trim();

			// Get Basket From BasketRepo
			var basket = await _basketRepository.GetBasketAsync(basketId);
			if (basket == null) return null;
			//Get SelectedItems at basket From ProductRepo
			var orderItems = new List<OrderItem>();
			if (basket?.Items?.Count > 0) {
				foreach (var item in basket.Items) {
				  var product = await _unitOfWork.Repository<Product>().GetAsync(item.Id);
				  var productItemOrdered = new ProductItemOrdered(item.Id, product.Name, product.PictureUrl);
				  var orderItem = new OrderItem(productItemOrdered, product.Price, item.Quantity);
				  orderItems.Add(orderItem);
				}
			}
			//Calculate SubTotal
			var subTotal = orderItems.Sum(orderItem=>orderItem.Price * orderItem.Quantity);
			//Get DeliveryMethod From DeliveryMethodsRepo
			var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetAsync(deliveryMethodId);
			var orderRepo = _unitOfWork.Repository<Order>();
			var orderSpec = new OrderWithPaymentIntentSpecifications(basket.PaymentIntentId);
			var existingOrder = await orderRepo.GetEntityWithSpecAsync(orderSpec);
			if (existingOrder != null) {
				orderRepo.Delete(existingOrder);
				await _unitOfWork.CompleteAsync();
				await _paymentService.CreateOrUpdatePaymentIntent(basketId, BuyerEmail);
				basket = await _basketRepository.GetBasketAsync(basketId);
				if (basket == null) return null;
			}
			//Create Order
			var order = new Order(BuyerEmail, ShippingAddress, deliveryMethod , orderItems , subTotal,basket.PaymentIntentId);

			if (!string.IsNullOrWhiteSpace(basket.PaymentIntentId)
				&& await _paymentService.IsPaymentIntentSucceededAsync(basket.PaymentIntentId))
			{
				order.Status = OrderStatus.PaymentReceived;
			}

			await orderRepo.AddAsync(order);
			var result = await _unitOfWork.CompleteAsync();
			if (result <= 0) return null;

			return order;

		}

		public async Task<IReadOnlyList<Order>> GetOrderForUserAsync(string buyerEmail)
		{
			if (string.IsNullOrWhiteSpace(buyerEmail))
				return Array.Empty<Order>();

			var ordersRepo =  _unitOfWork.Repository<Order>();
			var spec = new OrderSpecifications(buyerEmail.Trim());
			var orders = await ordersRepo.GetAllWithSpecAsync(spec);
			return orders;
		}
		public Task<Order?> GetOrderByIdForUserAsync(int OrderId, string buyerEmail)
		{
            var orderRepo = _unitOfWork.Repository<Order>();

			var orderSpec = new OrderSpecifications(OrderId, buyerEmail);
			var order = orderRepo.GetEntityWithSpecAsync(orderSpec);
			return order;
		}

		public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
		 => await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();
	}
}
