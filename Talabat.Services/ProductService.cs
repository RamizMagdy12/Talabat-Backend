using Talabat.Core.Entities;
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
}
