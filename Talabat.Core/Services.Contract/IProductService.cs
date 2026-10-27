

using Talabat.Core.Entities;
using Talabat.Core.Specifications;

namespace Talabat.Core.Services.Contract
{
   public interface IProductService
    {
        Task<IReadOnlyList<Product>> GetProductsAsync(ProductSpecParams specParams);
        Task<Product?> GetProductAsync(int productId);
        Task<IReadOnlyList<ProductBrand>> GetBrandsAsync();
        Task<IReadOnlyList<ProductCategory>> GetCategoriesAsync();
        Task<int> GetCountAsync(ProductSpecParams specParams);
        Task<IReadOnlyList<Product>> GetActiveOffersAsync();
        Task<IReadOnlyList<Product>> GetBestSellersAsync(int count = 8);
    }
}
