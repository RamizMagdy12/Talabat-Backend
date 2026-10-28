using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talabat.Admin.Localization;
using Talabat.Core;
using Talabat.Core.Entities;

using Talabat.Admin.Security;

namespace Talabat.Admin.Controllers
{
	[AdminPermission(AdminPermissions.Brands)]
	public class BrandsController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly SharedLocalizer _L;

		public BrandsController(IUnitOfWork unitOfWork, SharedLocalizer L)
		{
			_unitOfWork = unitOfWork;
			_L = L;
		}

		public async Task<IActionResult> Index(string? q)
		{
			var brands = await _unitOfWork.Repository<ProductBrand>().GetAllAsync();
			if (!string.IsNullOrWhiteSpace(q))
			{
				var term = q.Trim();
				brands = brands.Where(b =>
					(b.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) == true) ||
					(b.NameAr?.Contains(term, StringComparison.OrdinalIgnoreCase) == true)).ToList();
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
				ModelState.AddModelError(nameof(model.Name), _L["admin.msg.nameRequired"]);
				return View(model);
			}

			model.Name = model.Name.Trim();
			model.NameAr = string.IsNullOrWhiteSpace(model.NameAr) ? null : model.NameAr.Trim();
			await _unitOfWork.Repository<ProductBrand>().AddAsync(model);
			await _unitOfWork.CompleteAsync();
			TempData["Success"] = _L["admin.msg.brandCreated"];
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
				ModelState.AddModelError(nameof(model.Name), _L["admin.msg.nameRequired"]);
				return View(model);
			}

			var brand = await _unitOfWork.Repository<ProductBrand>().GetAsync(id);
			if (brand == null) return NotFound();

			brand.Name = model.Name.Trim();
			brand.NameAr = string.IsNullOrWhiteSpace(model.NameAr) ? null : model.NameAr.Trim();
			_unitOfWork.Repository<ProductBrand>().Update(brand);
			await _unitOfWork.CompleteAsync();
			TempData["Success"] = _L["admin.msg.brandUpdated"];
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
			TempData["Success"] = _L["admin.msg.brandDeleted"];
			return RedirectToAction(nameof(Index));
		}
	}
}
