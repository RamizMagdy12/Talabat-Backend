using AutoMapper;
using Talabat.API.Helper;
using Talabat.API.Mappings;
using Talabat.Core.Entities;

namespace Talabat.API.DTOs
{
	public class ProductToReturnDto:IMapFrom<Product>
	{
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Product, ProductToReturnDto>()
					.ForMember(d => d.Brand, opt => opt.MapFrom(s => s.ProductBrand.Name))
					.ForMember(d => d.Category, opt => opt.MapFrom(s => s.productCategory.Name))
					.ForMember(d=>d.PictureUrl,opt=>opt.MapFrom<ProductPictureUrlResolver>());
		}
		public int Id { get; set; }
		public string? Name { get; set; }
		public string? Description { get; set; }
		public string? PictureUrl { get; set; }
		public decimal Price { get; set; }
		public int BrandId { get; set; }
		public string Brand { get; set; }
		public int categoryId { get; set; }
		public string Category { get; set; }
	}
}
