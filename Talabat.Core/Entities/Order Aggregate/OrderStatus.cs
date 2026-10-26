using System.Runtime.Serialization;

namespace Talabat.Core.Entities.Order_Aggregate
{
	public enum OrderStatus
	{
		[EnumMember(Value = "Pending")]
		Pending = 0,

		[EnumMember(Value = "Payment Received")]
		PaymentReceived = 1,

		[EnumMember(Value = "Payment Failed")]
		PaymentFailed = 2,

		[EnumMember(Value = "Preparing")]
		Preparing = 3,

		[EnumMember(Value = "Out For Delivery")]
		OutForDelivery = 4,

		[EnumMember(Value = "Delivered")]
		Delivered = 5
	}
}
