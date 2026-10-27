using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Specifications;

namespace Talabat.Core.ProductSpec
{
   public class ProductWithFilterationForCountSpecifications:BaseSpecifications<Product>
    {
		public ProductWithFilterationForCountSpecifications(ProductSpecParams parameters) : base(p =>
		p.IsActive
		&&
		(string.IsNullOrEmpty(parameters.Search) || p.Name.ToLower().Contains(parameters.Search))
		&&
		(!parameters.brandId.HasValue || p.BrandId == parameters.brandId.Value)
		&&
		(!parameters.categoryId.HasValue || p.categoryId == parameters.categoryId.Value)
		)
		{
			
		}
	}
}
