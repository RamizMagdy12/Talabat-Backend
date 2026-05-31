using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.Core.Specifications.order_spec
{
   public class OrderWithPaymentIntentSpecifications : BaseSpecifications<Order>
    {
		public OrderWithPaymentIntentSpecifications(string paymentIntentId):base(
			o=>o.PaymentIntentId==paymentIntentId
			)
		{
			
		}
	}
}
