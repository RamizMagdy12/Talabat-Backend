namespace Talabat.Core.Entities
{
	public class BasketItem
	{
		public int Id { get; set; }
		public string ProductName { get;set; }
		public string ProductUrl { get; set; }
		public decimal Price { get; set; }
		public string category { get; set; }
		public string Brand { get; set; }
		public int Quantity { get; set; }
	}
}