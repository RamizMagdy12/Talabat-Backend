using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talabat.Admin.Models;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.ProductSpec;

using Talabat.Admin.Security;

namespace Talabat.Admin.Controllers
{
	[AdminPermission(AdminPermissions.Inventory)]
	public class InventoryController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

		public InventoryController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<IActionResult> Index(string? q)
		{
			var products = await _unitOfWork.Repository<Product>()
				.GetAllWithSpecAsync(new ProductWithBrandAndCategorySpecifications());
			var movements = await _unitOfWork.Repository<StockMovement>().GetAllAsync();

			if (!string.IsNullOrWhiteSpace(q))
			{
				var term = q.Trim();
				products = products.Where(p =>
					(p.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
					(p.ProductBrand?.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
					p.Id.ToString().Contains(term, StringComparison.OrdinalIgnoreCase)
				).ToList();
			}

			var byProduct = movements.GroupBy(m => m.ProductId).ToDictionary(g => g.Key, g => g.ToList());

			var rows = products
				.OrderBy(p => p.Name)
				.Select(p =>
				{
					byProduct.TryGetValue(p.Id, out var list);
					list ??= new List<StockMovement>();
					var added = list.Where(m => m.Type == StockMovementType.StockIn).Sum(m => m.Quantity);
					var sold = list.Where(m => m.Type == StockMovementType.Sale).Sum(m => m.Quantity);
					return new InventoryProductRow
					{
						ProductId = p.Id,
						Name = p.Name,
						Brand = p.ProductBrand?.Name,
						IsActive = p.IsActive,
						Remaining = p.Stock,
						TotalAdded = added,
						TotalSold = sold,
						LastInAtUtc = list.Where(m => m.Type == StockMovementType.StockIn)
							.Select(m => (DateTime?)m.OccurredAtUtc).DefaultIfEmpty().Max(),
						LastSoldAtUtc = list.Where(m => m.Type == StockMovementType.Sale)
							.Select(m => (DateTime?)m.OccurredAtUtc).DefaultIfEmpty().Max()
					};
				})
				.ToList();

			var model = new InventoryIndexViewModel
			{
				Search = q,
				Products = rows,
				TotalSkus = rows.Count,
				LowStockCount = rows.Count(r => r.Remaining <= 10),
				UnitsRemaining = rows.Sum(r => r.Remaining),
				UnitsSold = rows.Sum(r => r.TotalSold),
				UnitsAdded = rows.Sum(r => r.TotalAdded)
			};

			ViewBag.Search = q;
			return View(model);
		}

		public async Task<IActionResult> Details(int id)
		{
			var products = await _unitOfWork.Repository<Product>()
				.GetAllWithSpecAsync(new ProductWithBrandAndCategorySpecifications());
			var product = products.FirstOrDefault(p => p.Id == id);
			if (product == null) return NotFound();

			var movements = (await _unitOfWork.Repository<StockMovement>().GetAllAsync())
				.Where(m => m.ProductId == id)
				.OrderByDescending(m => m.OccurredAtUtc)
				.ThenByDescending(m => m.Id)
				.ToList();

			var model = new InventoryDetailsViewModel
			{
				ProductId = product.Id,
				Name = product.Name,
				Brand = product.ProductBrand?.Name,
				Remaining = product.Stock,
				TotalAdded = movements.Where(m => m.Type == StockMovementType.StockIn).Sum(m => m.Quantity),
				TotalSold = movements.Where(m => m.Type == StockMovementType.Sale).Sum(m => m.Quantity),
				TotalAdjustedOut = movements.Where(m => m.Type == StockMovementType.Adjustment).Sum(m => m.Quantity),
				Movements = movements.Select(m => new InventoryMovementRow
				{
					OccurredAtUtc = m.OccurredAtUtc,
					Type = m.Type,
					Quantity = m.Quantity,
					StockAfter = m.StockAfter,
					Note = m.Note,
					OrderId = m.OrderId
				}).ToList()
			};

			return View(model);
		}
	}
}
