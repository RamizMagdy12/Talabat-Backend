using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.ProductSpec;
using Talabat.Core.Services.Contract;
using Talabat.Core.Specifications;
using Talabat.Core;

public class ProductService : IProductService
{
	private readonly IUnitOfWork _unitOfWork;

	public ProductService(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<IReadOnlyList<Product>> GetProductsAsync(ProductSpecParams specParams)
	{
		var spec = new ProductWithBrandAndCategorySpecifications(specParams);
		return await _unitOfWork.Repository<Product>().GetAllWithSpecAsync(spec);
	}

	public async Task<int> GetCountAsync(ProductSpecParams specParams)
	{
		var countSpec = new ProductWithFilterationForCountSpecifications(specParams);
		return await _unitOfWork.Repository<Product>().GetCountAsync(countSpec);
	}

	public async Task<Product?> GetProductAsync(int productId)
	{
		var spec = new ProductWithBrandAndCategorySpecifications(productId);
		return await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(spec);
	}

	public async Task<IReadOnlyList<ProductBrand>> GetBrandsAsync()
		=> await _unitOfWork.Repository<ProductBrand>().GetAllAsync();

	public async Task<IReadOnlyList<ProductCategory>> GetCategoriesAsync()
		=> await _unitOfWork.Repository<ProductCategory>().GetAllAsync();

	public async Task<IReadOnlyList<Product>> GetActiveOffersAsync()
	{
		var products = await _unitOfWork.Repository<Product>()
			.GetAllWithSpecAsync(new ProductWithBrandAndCategorySpecifications());
		return products.Where(p => p.IsOfferActive()).ToList();
	}

	public async Task<IReadOnlyList<Product>> GetBestSellersAsync(int count = 8)
	{
		count = Math.Clamp(count, 1, 24);
		var orderItems = await _unitOfWork.Repository<OrderItem>().GetAllAsync();
		var rankedIds = orderItems
			.Where(i => i.Product != null)
			.GroupBy(i => i.Product.ProductId)
			.Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
			.OrderByDescending(x => x.Qty)
			.Take(count)
			.Select(x => x.ProductId)
			.ToList();

		var catalog = await _unitOfWork.Repository<Product>()
			.GetAllWithSpecAsync(new ProductWithBrandAndCategorySpecifications());

		if (rankedIds.Count == 0)
			return catalog.Take(count).ToList();

		var byId = catalog.ToDictionary(p => p.Id);
		var result = new List<Product>();
		foreach (var id in rankedIds)
		{
			if (byId.TryGetValue(id, out var product))
				result.Add(product);
		}

		if (result.Count < count)
		{
			foreach (var product in catalog)
			{
				if (result.Count >= count) break;
				if (result.All(p => p.Id != product.Id))
					result.Add(product);
			}
		}

		return result;
	}
}
