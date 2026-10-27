using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Talabat.Admin.Models
{
	public class ProductFormViewModel
	{
		public int Id { get; set; }

		[Required, MaxLength(100)]
		[Display(Name = "Name (English)")]
		public string Name { get; set; } = string.Empty;

		[MaxLength(100)]
		[Display(Name = "Name (Arabic)")]
		public string? NameAr { get; set; }

		[Required]
		[Display(Name = "Description (English)")]
		public string Description { get; set; } = string.Empty;

		[Display(Name = "Description (Arabic)")]
		public string? DescriptionAr { get; set; }

		public string PictureUrl { get; set; } = string.Empty;

		[Display(Name = "Product image")]
		public IFormFile? ImageFile { get; set; }

		[Required, Range(0.01, 100000)]
		public decimal Price { get; set; }

		[Required, Range(0, int.MaxValue)]
		public int Stock { get; set; }

		[Display(Name = "Visible on store")]
		public bool IsActive { get; set; } = true;

		[Required, Display(Name = "Brand")]
		public int BrandId { get; set; }

		[Required, Display(Name = "Category")]
		public int CategoryId { get; set; }

		public IEnumerable<SelectListItem> Brands { get; set; } = Enumerable.Empty<SelectListItem>();
		public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
	}
}
