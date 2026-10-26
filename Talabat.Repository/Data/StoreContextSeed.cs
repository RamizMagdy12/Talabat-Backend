using System.Text.Json;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.Repository.Data
{
   public static class StoreContextSeed
    {
        public async static Task SeedAsync(StoreContext storeContext)
        {
            if (storeContext.productBrands.Count() == 0)
            {
                var brandsData = File.ReadAllText("../Talabat.Repository/Data/DataSeeding/brands.json");
                var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandsData);
                if (brands?.Count() > 0)
                {
                    foreach(var brand in brands)
                    {
                        storeContext.Set<ProductBrand>().Add(brand);
                    }
                    await storeContext.SaveChangesAsync();
                }
            }
            if (storeContext.productCategories.Count() == 0)
            {
                var categoriesData = File.ReadAllText("../Talabat.Repository/Data/DataSeeding/categories.json");
                var categories = JsonSerializer.Deserialize<List<ProductCategory>>(categoriesData);
                if (categories?.Count() > 0)
                {
                    foreach(var category in categories)
                    {
                         storeContext.Set<ProductCategory>().Add(category);
                    }
                    await storeContext.SaveChangesAsync();
                }
            }
            if (storeContext.products.Count() == 0)
            {
                var productsData = File.ReadAllText("../Talabat.Repository/Data/DataSeeding/products.json");
                var products = JsonSerializer.Deserialize<List<Product>>(productsData);
                if (products?.Count() > 0)
                {
                    foreach(var product in products)
                    {
                        storeContext.Set<Product>().Add(product);
                    }
                    await storeContext.SaveChangesAsync();
                }
            }
            if (storeContext.deliveryMethods.Count()==0)
            {
                var DeliveryMethodData = File.ReadAllText("../Talabat.Repository/Data/DataSeeding/delivery.json");
                var DeliveryMethod = JsonSerializer.Deserialize<List<DeliveryMethod>>(DeliveryMethodData);

                if(DeliveryMethod?.Count() > 0)
                {
                    foreach(var deliveryMethod in DeliveryMethod)
                    {
                        storeContext.Set<DeliveryMethod>().Add(deliveryMethod);
                    }
                    await storeContext.SaveChangesAsync();
                }
            }

            // Backfill Stock for existing products that still have 0 after migration
            var productsWithoutStock = storeContext.products.Where(p => p.Stock <= 0).ToList();
            if (productsWithoutStock.Count > 0)
            {
                foreach (var product in productsWithoutStock)
                    product.Stock = 50;
                await storeContext.SaveChangesAsync();
            }
        }
    }
}
