using Talabat.API.Mappings;
using Talabat.Core.Entities;

namespace Talabat.API.DTOs
{
	public class BrandToReturnDto:IMapFrom<ProductBrand>
	{
		public int Id { get; set; }
		public string? Name { get; set; }
		public string? NameAr { get; set; }
	}
}
