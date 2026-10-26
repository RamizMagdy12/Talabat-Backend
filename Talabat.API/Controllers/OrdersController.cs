using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talabat.API.DTOs;
using Talabat.API.Errors;
using Talabat.API.Helper;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Services.Contract;

namespace Talabat.API.Controllers
{

    public class OrdersController : BaseAPIController
    {
		private readonly IOrderService _orderService;
		private readonly IMapper _mapper;

		public OrdersController(IOrderService orderService,IMapper mapper)
		{
			_orderService = orderService;
			_mapper = mapper;
		}
		[Authorize]
		[HttpPost]
		public async Task<ActionResult<OrderToReturnDto>> CreateOrder(OrderDto orderDto)
		{
			var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
			if (string.IsNullOrWhiteSpace(buyerEmail))
				return Unauthorized(new ApiResponse(401, "Buyer email is required"));

			var address = _mapper.Map<AddressDto, Address>(orderDto.ShippingAddress);
			var order = await _orderService.CreateOrderAsync(buyerEmail.Trim(), orderDto.BasketId, orderDto.DeliveryMethod, address);
			if (order == null) return BadRequest(new ApiResponse(400, "Unable to create order. Check stock availability and basket."));

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
			return Ok(mapped);
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
