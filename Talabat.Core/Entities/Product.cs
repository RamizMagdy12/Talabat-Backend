using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entities
{
   public class Product:BaseEntity
    {
        public string Name { get; set; }
        public string? NameAr { get; set; }
        public string Description { get; set; }
        public string? DescriptionAr { get; set; }
        public string PictureUrl { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        /// <summary>When false, product is hidden from the storefront.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Promotional price while the offer window is active.</summary>
        public decimal? OfferPrice { get; set; }
        public DateTime? OfferStartUtc { get; set; }
        public DateTime? OfferEndUtc { get; set; }

        public int BrandId { get; set; }
        public ProductBrand ProductBrand { get; set; }
        public int categoryId { get; set; }
        public ProductCategory productCategory { get; set; }

        public bool IsOfferActive(DateTime? utcNow = null)
        {
            var now = utcNow ?? DateTime.UtcNow;
            return OfferPrice.HasValue
                && OfferPrice.Value > 0
                && OfferPrice.Value < Price
                && OfferStartUtc.HasValue
                && OfferEndUtc.HasValue
                && OfferStartUtc.Value <= now
                && OfferEndUtc.Value >= now;
        }

        public decimal GetEffectivePrice(DateTime? utcNow = null)
            => IsOfferActive(utcNow) ? OfferPrice!.Value : Price;
    }
}
