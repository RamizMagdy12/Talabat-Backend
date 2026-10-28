using System.Security.Cryptography;
using System.Text;

namespace Talabat.Core.Entities.Order_Aggregate
{
	public static class OrderNumberGenerator
	{
		private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

		/// <summary>Format: TLB-yyMMdd-XXXX (unique public order code).</summary>
		public static string Create(DateTimeOffset? utcNow = null)
		{
			var now = (utcNow ?? DateTimeOffset.UtcNow).UtcDateTime;
			var suffix = new StringBuilder(4);
			Span<byte> bytes = stackalloc byte[4];
			RandomNumberGenerator.Fill(bytes);
			for (var i = 0; i < 4; i++)
				suffix.Append(Alphabet[bytes[i] % Alphabet.Length]);

			return $"TLB-{now:yyMMdd}-{suffix}";
		}
	}
}
