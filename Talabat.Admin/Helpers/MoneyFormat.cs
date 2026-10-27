using System.Globalization;

namespace Talabat.Admin.Helpers
{
	public static class MoneyFormat
	{
		private static readonly CultureInfo MoneyCulture = CultureInfo.GetCultureInfo("en-US");

		public static string Format(decimal value) => value.ToString("C", MoneyCulture);
	}
}
