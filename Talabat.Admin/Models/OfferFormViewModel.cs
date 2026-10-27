using System.ComponentModel.DataAnnotations;

namespace Talabat.Admin.Models
{
	public class OfferFormViewModel
	{
		public int ProductId { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public decimal RegularPrice { get; set; }
		public string? PictureUrl { get; set; }

		[Display(Name = "Offer price")]
		[Range(0.01, 100000)]
		public decimal? OfferPrice { get; set; }

		[Display(Name = "Offer starts")]
		public DateTime? OfferStartLocal { get; set; }

		[Display(Name = "Offer ends")]
		public DateTime? OfferEndLocal { get; set; }

		public bool ClearOffer { get; set; }
	}
}
