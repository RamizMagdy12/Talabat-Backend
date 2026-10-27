using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Talabat.Admin.Models;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.ProductSpec;

namespace Talabat.Admin.Controllers
{
	[Authorize(Roles = "Admin")]
	public class ProductsController : Controller
	{
		private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
		{
			".jpg", ".jpeg", ".png", ".webp", ".gif"
		};

		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _env;
		private readonly IConfiguration _configuration;

		public ProductsController(IUnitOfWork unitOfWork, IWebHostEnvironment env, IConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_env = env;
			_configuration = configuration;
		}

		public async Task<IActionResult> Index(string? q)
		{
			var spec = new ProductWithBrandAndCategorySpecifications();
			var products = await _unitOfWork.Repository<Product>().GetAllWithSpecAsync(spec);
			if (!string.IsNullOrWhiteSpace(q))
			{
				var term = q.Trim();
				products = products.Where(p =>
					(p.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
					(p.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
					(p.ProductBrand?.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
					(p.productCategory?.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
					p.Id.ToString().Contains(term, StringComparison.OrdinalIgnoreCase)
				).ToList();
			}

			ViewBag.Search = q;
			return View(products);
		}

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			return View(await BuildFormAsync(new ProductFormViewModel()));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[RequestSizeLimit(10 * 1024 * 1024)]
		public async Task<IActionResult> Create(ProductFormViewModel model)
		{
			if (model.ImageFile == null || model.ImageFile.Length == 0)
				ModelState.AddModelError(nameof(model.ImageFile), "Please upload a product image.");

			if (!ModelState.IsValid)
				return View(await BuildFormAsync(model));

			var pictureUrl = await SaveImageAsync(model.ImageFile!);
			if (pictureUrl == null)
			{
				ModelState.AddModelError(nameof(model.ImageFile), "Invalid image. Use JPG, PNG, WEBP, or GIF (max 10 MB).");
				return View(await BuildFormAsync(model));
			}

			var product = new Product
			{
				Name = model.Name,
				Description = model.Description,
				PictureUrl = pictureUrl,
				Price = model.Price,
				Stock = Math.Max(0, model.Stock),
				IsActive = model.IsActive,
				BrandId = model.BrandId,
				categoryId = model.CategoryId
			};

			await _unitOfWork.Repository<Product>().AddAsync(product);
			await _unitOfWork.CompleteAsync();

			if (product.Stock > 0)
			{
				await _unitOfWork.Repository<StockMovement>().AddAsync(new StockMovement
				{
					ProductId = product.Id,
					Type = StockMovementType.StockIn,
					Quantity = product.Stock,
					StockAfter = product.Stock,
					OccurredAtUtc = DateTime.UtcNow,
					Note = "Initial stock on create",
					CreatedDateTime = DateTime.UtcNow
				});
				await _unitOfWork.CompleteAsync();
			}

			TempData["Success"] = "Product created.";
			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var product = await _unitOfWork.Repository<Product>().GetAsync(id);
			if (product == null) return NotFound();

			var model = new ProductFormViewModel
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				PictureUrl = product.PictureUrl,
				Price = product.Price,
				Stock = product.Stock,
				IsActive = product.IsActive,
				BrandId = product.BrandId,
				CategoryId = product.categoryId
			};

			return View(await BuildFormAsync(model));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[RequestSizeLimit(10 * 1024 * 1024)]
		public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
		{
			if (id != model.Id) return BadRequest();
			if (!ModelState.IsValid)
				return View(await BuildFormAsync(model));

			var product = await _unitOfWork.Repository<Product>().GetAsync(id);
			if (product == null) return NotFound();

			if (model.ImageFile != null && model.ImageFile.Length > 0)
			{
				var pictureUrl = await SaveImageAsync(model.ImageFile);
				if (pictureUrl == null)
				{
					ModelState.AddModelError(nameof(model.ImageFile), "Invalid image. Use JPG, PNG, WEBP, or GIF (max 10 MB).");
					return View(await BuildFormAsync(model));
				}
				product.PictureUrl = pictureUrl;
			}

			var previousStock = product.Stock;
			var nextStock = Math.Max(0, model.Stock);

			product.Name = model.Name;
			product.Description = model.Description;
			product.Price = model.Price;
			product.Stock = nextStock;
			product.IsActive = model.IsActive;
			product.BrandId = model.BrandId;
			product.categoryId = model.CategoryId;

			_unitOfWork.Repository<Product>().Update(product);

			var delta = nextStock - previousStock;
			if (delta != 0)
			{
				await _unitOfWork.Repository<StockMovement>().AddAsync(new StockMovement
				{
					ProductId = product.Id,
					Type = delta > 0 ? StockMovementType.StockIn : StockMovementType.Adjustment,
					Quantity = Math.Abs(delta),
					StockAfter = nextStock,
					OccurredAtUtc = DateTime.UtcNow,
					Note = delta > 0
						? $"Stock added (+{delta})"
						: $"Stock adjusted ({delta})",
					CreatedDateTime = DateTime.UtcNow
				});
			}

			await _unitOfWork.CompleteAsync();
			TempData["Success"] = "Product updated.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ToggleActive(int id)
		{
			var product = await _unitOfWork.Repository<Product>().GetAsync(id);
			if (product == null) return NotFound();

			product.IsActive = !product.IsActive;
			_unitOfWork.Repository<Product>().Update(product);
			await _unitOfWork.CompleteAsync();
			TempData["Success"] = product.IsActive
				? $"{product.Name} is now visible on the store."
				: $"{product.Name} is now hidden from the store.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			var product = await _unitOfWork.Repository<Product>().GetAsync(id);
			if (product == null) return NotFound();

			_unitOfWork.Repository<Product>().Delete(product);
			await _unitOfWork.CompleteAsync();
			TempData["Success"] = "Product deleted.";
			return RedirectToAction(nameof(Index));
		}

		private async Task<ProductFormViewModel> BuildFormAsync(ProductFormViewModel model)
		{
			var brands = await _unitOfWork.Repository<ProductBrand>().GetAllAsync();
			var categories = await _unitOfWork.Repository<ProductCategory>().GetAllAsync();

			model.Brands = brands.Select(b => new SelectListItem(b.Name, b.Id.ToString(), b.Id == model.BrandId));
			model.Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.CategoryId));
			return model;
		}

		private async Task<string?> SaveImageAsync(IFormFile file)
		{
			if (file.Length <= 0 || file.Length > 10 * 1024 * 1024)
				return null;

			var extension = Path.GetExtension(file.FileName);
			if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
				return null;

			var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
			var relativePath = $"images/products/{fileName}";

			foreach (var folder in GetImageFolders())
			{
				Directory.CreateDirectory(folder);
				var fullPath = Path.Combine(folder, fileName);
				await using var stream = System.IO.File.Create(fullPath);
				await file.CopyToAsync(stream);
			}

			return relativePath.Replace('\\', '/');
		}

		private IEnumerable<string> GetImageFolders()
		{
			var folders = new List<string>
			{
				Path.Combine(_env.WebRootPath, "images", "products")
			};

			var apiImages = _configuration["ProductImagesPath"];
			if (string.IsNullOrWhiteSpace(apiImages))
				apiImages = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..", "Talabat.API", "wwwroot", "images", "products"));
			else if (!Path.IsPathRooted(apiImages))
				apiImages = Path.GetFullPath(Path.Combine(_env.ContentRootPath, apiImages));

			if (!folders.Any(f => string.Equals(f, apiImages, StringComparison.OrdinalIgnoreCase)))
				folders.Add(apiImages);

			return folders;
		}
	}
}
