namespace Talabat.API.Helper
{
	public class Pagination<T>
	{
		public int PageIndex { get; set; }
		public int PageSize { get; set; }
		public int Count { get; set; }
		public IReadOnlyList<T> Items { get; set; }

		public Pagination(int pageIndex, int pageSize, int count, IReadOnlyList<T>items)
		{
			PageIndex = pageIndex;
			PageSize = pageSize;
			Count = count;
			Items = items;
		}
	}
}
