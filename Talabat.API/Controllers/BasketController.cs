using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.API.DTOs;
using Talabat.API.Errors;
using Talabat.Core.Entities;
using Talabat.Core.Repositories.Contract;

namespace Talabat.API.Controllers
{

    public class BasketController : BaseAPIController
    {
		private readonly IBasketRepository _basketRepository;
		private readonly IMapper _mapper;

		public BasketController(IBasketRepository basketRepository,IMapper mapper)
		{
			_basketRepository = basketRepository;
			_mapper = mapper;
		}
		[HttpGet]
		public async Task<ActionResult<CustomerBasket>> Basket(string basketid)
		{
			var basket = await _basketRepository.GetBasketAsync(basketid);
			return basket ?? new CustomerBasket(basketid);
		}
		[HttpPost]
		public async Task<ActionResult<CustomerBasket>>UpdateBasket(CustomerBasketDto basket)
		{
			var Mapped = _mapper.Map<CustomerBasketDto, CustomerBasket>(basket);
			var CreatedOrUpdate = await _basketRepository.UpdateBasketAsync(Mapped);
			if (CreatedOrUpdate is null)
				return BadRequest(new ApiResponse(400));

			return Ok(CreatedOrUpdate);
		}
		[HttpDelete]
			public async Task DeleteBasket(string basketId)
		{
			await _basketRepository.DeleteBasketAsync(basketId);
		}
	}
}
