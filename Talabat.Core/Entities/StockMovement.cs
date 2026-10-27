namespace Talabat.Core.Entities
{
	public class StockMovement : BaseEntity
	{
		public int ProductId { get; set; }
		public Product Product { get; set; } = null!;

		public StockMovementType Type { get; set; }

		/// <summary>Always a positive quantity for the movement.</summary>
		public int Quantity { get; set; }

		public int StockAfter { get; set; }

		public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

		public string? Note { get; set; }

		public int? OrderId { get; set; }
	}
}
