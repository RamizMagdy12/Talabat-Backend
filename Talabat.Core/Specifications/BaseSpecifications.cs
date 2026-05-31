using System.Linq.Expressions;
using Talabat.Core.Entities;

namespace Talabat.Core.Specifications
{
	public class BaseSpecifications<T> : ISpecifications<T> where T : BaseEntity
	{
		public Expression<Func<T, bool>> criteria { get; set; } = null!;
		public List<Expression<Func<T, object>>> Includes { get; set; } = new List<Expression<Func<T, object>>>();
		public Expression<Func<T, object>> orderBy { get ; set  ; }
		public Expression<Func<T, object>> orderByDesc { get ; set ; }
		public int Skip { get ; set ; }
		public int Take { get; set ; }
		public bool IsPaginationEnabled { get ; set ; }

		public BaseSpecifications()
		{
			
		}
		public BaseSpecifications(Expression<Func<T, bool>> criteriaExpression)
		{
			criteria = criteriaExpression;
		}

		public void AddOrderBy(Expression<Func<T, object>> orderByExpression)
		{
			orderBy = orderByExpression;
		}
		public void AddOrderByDesc( Expression<Func<T, object>> orderByDescExpression)
		{
			orderByDesc = orderByDescExpression;
		}
		public void ApplyPagination(int skip , int take)
		{
			IsPaginationEnabled = true;
			Skip = skip;
			Take = take;
		}
	}
}
