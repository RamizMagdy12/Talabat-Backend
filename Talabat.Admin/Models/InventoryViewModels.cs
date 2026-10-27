using Talabat.Core.Entities;

namespace Talabat.Admin.Models
{
	public class InventoryIndexViewModel
	{
		public string? Search { get; set; }
		public int TotalSkus { get; set; }
		public int LowStockCount { get; set; }
		public int UnitsRemaining { get; set; }
		public int UnitsSold { get; set; }
		public int UnitsAdded { get; set; }
		public List<InventoryProductRow> Products { get; set; } = new();
	}

	public class InventoryProductRow
	{
		public int ProductId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Brand { get; set; }
		public bool IsActive { get; set; }
		public int Remaining { get; set; }
		public int TotalAdded { get; set; }
		public int TotalSold { get; set; }
		public DateTime? LastInAtUtc { get; set; }
		public DateTime? LastSoldAtUtc { get; set; }
	}

	public class InventoryDetailsViewModel
	{
		public int ProductId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Brand { get; set; }
		public int Remaining { get; set; }
		public int TotalAdded { get; set; }
		public int TotalSold { get; set; }
		public int TotalAdjustedOut { get; set; }
		public List<InventoryMovementRow> Movements { get; set; } = new();
	}

	public class InventoryMovementRow
	{
		public DateTime OccurredAtUtc { get; set; }
		public StockMovementType Type { get; set; }
		public int Quantity { get; set; }
		public int StockAfter { get; set; }
		public string? Note { get; set; }
		public int? OrderId { get; set; }

		public string TypeLabel
		{
			get
			{
				var ar = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
					.Equals("ar", StringComparison.OrdinalIgnoreCase);
				return Type switch
				{
					StockMovementType.StockIn => ar ? "إضافة مخزون" : "Stock in",
					StockMovementType.Sale => ar ? "بيع" : "Sold",
					StockMovementType.Adjustment => ar ? "تعديل" : "Adjustment",
					_ => Type.ToString()
				};
			}
		}
	}
}
