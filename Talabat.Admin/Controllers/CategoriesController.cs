using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talabat.Admin.Localization;
using Talabat.Core;
using Talabat.Core.Entities;

namespace Talabat.Admin.Controllers
{
	[Authorize(Roles = "Admin")]
	public class CategoriesController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly SharedLocalizer _L;

		public CategoriesController(IUnitOfWork unitOfWork, SharedLocalizer L)
		{
			_unitOfWork = unitOfWork;
			_L = L;
		}

		public async Task<IActionResult> Index(string? q)
		{
			var categories = await _unitOfWork.Repository<ProductCategory>().GetAllAsync();
			if (!string.IsNullOrWhiteSpace(q))
			{
				var term = q.Trim();
				categories = categories.Where(c =>
					(c.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) == true) ||
					(c.NameAr?.Contains(term, StringComparison.OrdinalIgnoreCase) == true)).ToList();
			}

			ViewBag.Search = q;
			return View(categories);
		}

		[HttpGet]
		public IActionResult Create() => View(new ProductCategory());

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ProductCategory model)
		{
			if (string.IsNullOrWhiteSpace(model.Name))
			{
				ModelState.AddModelError(nameof(model.Name), _L["admin.msg.nameRequired"]);
				return View(model);
			}

			model.Name = model.Name.Trim();
			model.NameAr = string.IsNullOrWhiteSpace(model.NameAr) ? null : model.NameAr.Trim();
			await _unitOfWork.Repository<ProductCategory>().AddAsync(model);
			await _unitOfWork.CompleteAsync();
			TempData["Success"] = _L["admin.msg.categoryCreated"];
			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var category = await _unitOfWork.Repository<ProductCategory>().GetAsync(id);
			if (category == null) return NotFound();
			return View(category);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, ProductCategory model)
		{
			if (id != model.Id) return BadRequest();
			if (string.IsNullOrWhiteSpace(model.Name))
			{
				ModelState.AddModelError(nameof(model.Name), _L["admin.msg.nameRequired"]);
				return View(model);
			}

			var category = await _unitOfWork.Repository<ProductCategory>().GetAsync(id);
			if (category == null) return NotFound();

			category.Name = model.Name.Trim();
			category.NameAr = string.IsNullOrWhiteSpace(model.NameAr) ? null : model.NameAr.Trim();
			_unitOfWork.Repository<ProductCategory>().Update(category);
			await _unitOfWork.CompleteAsync();
			TempData["Success"] = _L["admin.msg.categoryUpdated"];
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			var category = await _unitOfWork.Repository<ProductCategory>().GetAsync(id);
			if (category == null) return NotFound();

			_unitOfWork.Repository<ProductCategory>().Delete(category);
			await _unitOfWork.CompleteAsync();
			TempData["Success"] = _L["admin.msg.categoryDeleted"];
			return RedirectToAction(nameof(Index));
		}
	}
}
