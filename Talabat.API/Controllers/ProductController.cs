using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Talabat.API.Controllers;
using Talabat.API.DTOs;
using Talabat.API.Helper;
using Talabat.Core.Services.Contract;
using Talabat.Core.Specifications;

public class ProductController : BaseAPIController
{
	private readonly IProductService _productService;
	private readonly IMapper _mapper;

	public ProductController(IProductService productService, IMapper mapper)
	{
		_productService = productService;
		_mapper = mapper;
	}
	[Cached]
	[HttpGet]
	public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts([FromQuery] ProductSpecParams parameters)
	{
		var products = await _productService.GetProductsAsync(parameters);
		var count = await _productService.GetCountAsync(parameters);

		var mapped = _mapper.Map<IReadOnlyList<ProductToReturnDto>>(products);

		return Ok(new Pagination<ProductToReturnDto>(
			parameters.PageIndex,
			parameters.PageSize,
			count,
			mapped
		));
	}
	[Cached]
	[HttpGet("{id}")]
	public async Task<ActionResult<ProductToReturnDto>> GetProductById(int id)
	{
		var product = await _productService.GetProductAsync(id);

		if (product == null)
			return NotFound(new { Message = "Not Found", StatusCode = 404 });

		var mapped = _mapper.Map<ProductToReturnDto>(product);

		return Ok(mapped);
	}
	[Cached]
	[HttpGet("brands")]
	public async Task<ActionResult<IEnumerable<BrandToReturnDto>>> GetBrands()
	{
		var brands = await _productService.GetBrandsAsync();
		var mapped = _mapper.Map<IEnumerable<BrandToReturnDto>>(brands);

		return Ok(mapped);
	}
	[Cached]
	[HttpGet("categories")]
	public async Task<ActionResult<IEnumerable<CategoryToReturnDto>>> GetCategories()
	{
		var categories = await _productService.GetCategoriesAsync();
		var mapped = _mapper.Map<IEnumerable<CategoryToReturnDto>>(categories);

		return Ok(mapped);
	}
}
