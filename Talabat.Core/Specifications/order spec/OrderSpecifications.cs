
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.Core.Specifications.order_spec
{
   public class OrderSpecifications:BaseSpecifications<Order>
    {
		public OrderSpecifications(string buyerEmail):
			base(o=>o.BuyerEmail == buyerEmail) 
		{
			Includes.Add(o => o.DeliveryMethod);
			Includes.Add(o => o.Items);

			AddOrderByDesc(o => o.OrderDate);
		}
		public OrderSpecifications(int id , string buyerEmail): 
			base(o=>o.Id == id && o.BuyerEmail == buyerEmail) 
		{
			Includes.Add(o => o.DeliveryMethod);
			Includes.Add(o => o.Items);
		}

		/// <summary>Admin: all orders newest first.</summary>
		public OrderSpecifications() : base()
		{
			Includes.Add(o => o.DeliveryMethod);
			Includes.Add(o => o.Items);
			AddOrderByDesc(o => o.OrderDate);
		}

		/// <summary>Admin: order by id.</summary>
		public OrderSpecifications(int id) : base(o => o.Id == id)
		{
			Includes.Add(o => o.DeliveryMethod);
			Includes.Add(o => o.Items);
		}
	}
}
