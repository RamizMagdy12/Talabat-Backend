using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talabat.API.DTOs;
using Talabat.API.Errors;
using Talabat.API.Helper;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Services.Contract;

namespace Talabat.API.Controllers
{

    public class OrdersController : BaseAPIController
    {
		private readonly IOrderService _orderService;
		private readonly IMapper _mapper;
		private readonly UserManager<AppUser> _userManager;
		private readonly IEmailService _emailService;
		private readonly ILogger<OrdersController> _logger;

		public OrdersController(
			IOrderService orderService,
			IMapper mapper,
			UserManager<AppUser> userManager,
			IEmailService emailService,
			ILogger<OrdersController> logger)
		{
			_orderService = orderService;
			_mapper = mapper;
			_userManager = userManager;
			_emailService = emailService;
			_logger = logger;
		}
		[Authorize]
		[HttpPost]
		public async Task<ActionResult<OrderToReturnDto>> CreateOrder(OrderDto orderDto)
		{
			var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
			if (string.IsNullOrWhiteSpace(buyerEmail))
				return Unauthorized(new ApiResponse(401, "Buyer email is required"));

			var address = _mapper.Map<AddressDto, Talabat.Core.Entities.Order_Aggregate.Address>(orderDto.ShippingAddress);
			if (string.IsNullOrWhiteSpace(address.PhoneNumber))
			{
				var buyer = await _userManager.FindByEmailAsync(buyerEmail.Trim());
				address.PhoneNumber = buyer?.PhoneNumber?.Trim() ?? string.Empty;
			}

			var order = await _orderService.CreateOrderAsync(buyerEmail.Trim(), orderDto.BasketId, orderDto.DeliveryMethod, address);
			if (order == null) return BadRequest(new ApiResponse(400, "Unable to create order. Check stock availability and basket."));

			try
			{
				var html = OrderConfirmationEmail.BuildHtml(order);
				await _emailService.SendAsync(
					order.BuyerEmail,
					$"Talabat — order #{order.Id} confirmed",
					html);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Order #{OrderId} created but confirmation email failed", order.Id);
			}

			var mapped = _mapper.Map<Order, OrderToReturnDto>(order);
			return Ok(mapped);
		}
		[Authorize]
		[HttpGet]
		public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetOrdersForUser()
		{
			var email = User.FindFirstValue(ClaimTypes.Email);
			if (string.IsNullOrWhiteSpace(email))
				return Unauthorized(new ApiResponse(401, "Buyer email is required"));

			var orders = await _orderService.GetOrderForUserAsync(email.Trim());
			var mapped = _mapper.Map<IReadOnlyList<Order>,IReadOnlyList<OrderToReturnDto>>(orders);
			await FillMissingPhonesAsync(mapped, email.Trim());

			return Ok(mapped);
		}
		[Authorize]
		[HttpGet("{id}")]
		public async Task<ActionResult<OrderToReturnDto>>GetOrderIdForUser(int id )
		{
			var email = User.FindFirstValue(ClaimTypes.Email);
			if (string.IsNullOrWhiteSpace(email))
				return Unauthorized(new ApiResponse(401, "Buyer email is required"));

			var order = await _orderService.GetOrderByIdForUserAsync(id , email.Trim());

			if (order == null) return NotFound(new ApiResponse(404));

			var mapped = _mapper.Map<Order,OrderToReturnDto>(order);
			await FillMissingPhonesAsync(new[] { mapped }, email.Trim());
			return Ok(mapped);
		}

		private async Task FillMissingPhonesAsync(IEnumerable<OrderToReturnDto> orders, string email)
		{
			var needsPhone = orders.Any(o =>
				o.ShippingAddress != null && string.IsNullOrWhiteSpace(o.ShippingAddress.PhoneNumber));
			if (!needsPhone) return;

			var buyer = await _userManager.FindByEmailAsync(email);
			var phone = buyer?.PhoneNumber?.Trim();
			if (string.IsNullOrWhiteSpace(phone)) return;

			foreach (var order in orders)
			{
				if (order.ShippingAddress != null && string.IsNullOrWhiteSpace(order.ShippingAddress.PhoneNumber))
					order.ShippingAddress.PhoneNumber = phone;
			}
		}
		[Cached]
		[HttpGet("deliveryMethod")]
		public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
		{
			var deliveryMethod = await _orderService.GetDeliveryMethodsAsync();
			return Ok(deliveryMethod);
		}
	}
}
