using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Specifications;

namespace Talabat.Core.ProductSpec
{
   public class ProductWithBrandAndCategorySpecifications: BaseSpecifications<Product>
    {
		public ProductWithBrandAndCategorySpecifications(ProductSpecParams parameters):base(p=>
		p.IsActive
		&&
		(string.IsNullOrEmpty(parameters.Search)||p.Name.ToLower().Contains(parameters.Search))
		&&
		(!parameters.brandId.HasValue || p.BrandId == parameters.brandId.Value)
		&&
		(!parameters.categoryId.HasValue || p.categoryId == parameters.categoryId.Value)
		)
		{
			AddIncludes();
			if (!string.IsNullOrEmpty(parameters.sort))
			{
				switch (parameters.sort)
				{
					case "PriceAsc":
						AddOrderBy(p => p.Price);
						break;
					case "PriceDesc":
						AddOrderByDesc(p => p.Price);
						break;

					default:
						AddOrderBy(p => p.Name);
						break;

				}
			}
			else
			{
				AddOrderBy(p => p.Name);
			}
			ApplyPagination((parameters.PageIndex - 1) * parameters.PageSize, parameters.PageSize);
		}
		public ProductWithBrandAndCategorySpecifications(int id):base(p=>p.Id==id && p.IsActive)
		{
			AddIncludes();
		}

		/// <summary>Admin: all products with brand/category, no pagination.</summary>
		public ProductWithBrandAndCategorySpecifications() : base()
		{
			criteria = null!;
			AddIncludes();
			AddOrderBy(p => p.Name);
		}

		// helper
		private void AddIncludes()
		{
			Includes.Add(p => p.ProductBrand);
			Includes.Add(p => p.productCategory);
		}
	}
}
