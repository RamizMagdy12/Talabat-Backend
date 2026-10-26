using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talabat.Core;
using Talabat.Core.Entities;

namespace Talabat.Admin.Controllers
{
	[Authorize(Roles = "Admin")]
	public class BrandsController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

		public BrandsController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<IActionResult> Index(string? q)
		{
			var brands = await _unitOfWork.Repository<ProductBrand>().GetAllAsync();
			if (!string.IsNullOrWhiteSpace(q))
			{
				var term = q.Trim();
				brands = brands.Where(b => b.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) == true).ToList();
			}

			ViewBag.Search = q;
			return View(brands);
		}

		[HttpGet]
		public IActionResult Create() => View(new ProductBrand());

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ProductBrand model)
		{
			if (string.IsNullOrWhiteSpace(model.Name))
			{
				ModelState.AddModelError(nameof(model.Name), "Name is required");
				return View(model);
			}

			await _unitOfWork.Repository<ProductBrand>().AddAsync(model);
			await _unitOfWork.CompleteAsync();
			TempData["Success"] = "Brand created.";
			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var brand = await _unitOfWork.Repository<ProductBrand>().GetAsync(id);
			if (brand == null) return NotFound();
			return View(brand);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, ProductBrand model)
		{
			if (id != model.Id) return BadRequest();
			if (string.IsNullOrWhiteSpace(model.Name))
			{
				ModelState.AddModelError(nameof(model.Name), "Name is required");
				return View(model);
			}

			var brand = await _unitOfWork.Repository<ProductBrand>().GetAsync(id);
			if (brand == null) return NotFound();

			brand.Name = model.Name;
			_unitOfWork.Repository<ProductBrand>().Update(brand);
			await _unitOfWork.CompleteAsync();
			TempData["Success"] = "Brand updated.";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			var brand = await _unitOfWork.Repository<ProductBrand>().GetAsync(id);
			if (brand == null) return NotFound();

			_unitOfWork.Repository<ProductBrand>().Delete(brand);
			await _unitOfWork.CompleteAsync();
			TempData["Success"] = "Brand deleted.";
			return RedirectToAction(nameof(Index));
		}
	}
}
