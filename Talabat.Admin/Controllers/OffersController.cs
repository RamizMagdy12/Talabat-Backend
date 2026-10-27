using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talabat.Admin.Localization;
using Talabat.Admin.Models;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.ProductSpec;

namespace Talabat.Admin.Controllers
{
	[Authorize(Roles = "Admin")]
	public class OffersController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly SharedLocalizer _L;

		public OffersController(IUnitOfWork unitOfWork, SharedLocalizer L)
		{
			_unitOfWork = unitOfWork;
			_L = L;
		}

		public async Task<IActionResult> Index(string? q)
		{
			var products = await _unitOfWork.Repository<Product>()
				.GetAllWithSpecAsync(new ProductWithBrandAndCategorySpecifications());

			if (!string.IsNullOrWhiteSpace(q))
			{
				var term = q.Trim();
				products = products.Where(p =>
					(p.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
					p.Id.ToString().Contains(term, StringComparison.OrdinalIgnoreCase)
				).ToList();
			}

			ViewBag.Search = q;
			return View(products);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var product = await _unitOfWork.Repository<Product>().GetAsync(id);
			if (product == null) return NotFound();

			return View(ToForm(product));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, OfferFormViewModel model)
		{
			if (id != model.ProductId) return BadRequest();

			var product = await _unitOfWork.Repository<Product>().GetAsync(id);
			if (product == null) return NotFound();

			model.ProductName = product.Name;
			model.RegularPrice = product.Price;
			model.PictureUrl = product.PictureUrl;

			if (model.ClearOffer)
			{
				product.OfferPrice = null;
				product.OfferStartUtc = null;
				product.OfferEndUtc = null;
				_unitOfWork.Repository<Product>().Update(product);
				await _unitOfWork.CompleteAsync();
				TempData["Success"] = _L.Get("admin.msg.offerCleared", ("name", product.Name));
				return RedirectToAction(nameof(Index));
			}

			if (!model.OfferPrice.HasValue)
				ModelState.AddModelError(nameof(model.OfferPrice), "Enter an offer price.");
			else if (model.OfferPrice.Value <= 0 || model.OfferPrice.Value >= product.Price)
				ModelState.AddModelError(nameof(model.OfferPrice), $"Offer price must be greater than 0 and less than {product.Price:C}.");

			if (!model.OfferStartLocal.HasValue)
				ModelState.AddModelError(nameof(model.OfferStartLocal), "Start date is required.");
			if (!model.OfferEndLocal.HasValue)
				ModelState.AddModelError(nameof(model.OfferEndLocal), "End date is required.");
			if (model.OfferStartLocal.HasValue && model.OfferEndLocal.HasValue
				&& model.OfferEndLocal.Value <= model.OfferStartLocal.Value)
				ModelState.AddModelError(nameof(model.OfferEndLocal), "End must be after start.");

			if (!ModelState.IsValid)
				return View(model);

			product.OfferPrice = model.OfferPrice;
			product.OfferStartUtc = model.OfferStartLocal!.Value.ToUniversalTime();
			product.OfferEndUtc = model.OfferEndLocal!.Value.ToUniversalTime();

			_unitOfWork.Repository<Product>().Update(product);
			await _unitOfWork.CompleteAsync();
			TempData["Success"] = _L.Get("admin.msg.offerSaved", ("name", product.Name));
			return RedirectToAction(nameof(Index));
		}

		private static OfferFormViewModel ToForm(Product product) => new()
		{
			ProductId = product.Id,
			ProductName = product.Name,
			RegularPrice = product.Price,
			PictureUrl = product.PictureUrl,
			OfferPrice = product.OfferPrice,
			OfferStartLocal = product.OfferStartUtc?.ToLocalTime(),
			OfferEndLocal = product.OfferEndUtc?.ToLocalTime()
		};
	}
}
