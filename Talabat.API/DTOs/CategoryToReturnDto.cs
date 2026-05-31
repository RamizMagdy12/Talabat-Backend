using Talabat.API.Mappings;
using Talabat.Core.Entities;

namespace Talabat.API.DTOs
{
	public class CategoryToReturnDto:IMapFrom<ProductCategory>
	{
		public int Id { get; set; }
		public string? Name { get; set; }
	}
}
