using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.Core.Services.Contract
{
   public interface IPaymentService
    {
        Task<CustomerBasket?> CreateOrUpdatePaymentIntent(string basketId, string buyerEmail);
        Task<bool> IsPaymentIntentSucceededAsync(string paymentIntentId);
        Task<Order?> UpdatePaymentIntentToSucceedOrFaild(string paymentIntentId, bool isSucceeded);
    }
}
