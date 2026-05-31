using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Specifications;

namespace Talabat.Repository.Specifications
{
  internal static  class SpecificationsEvaluator<T> where T : BaseEntity
    {
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery , ISpecifications<T> spec)
        {
            var query = inputQuery; // _db.Set<Product>()
            if (spec.criteria is not null)
                query = query.Where(spec.criteria);

            query = spec.Includes.Aggregate(query, (currentQuery, includeExpression) => currentQuery.Include(includeExpression));

            if (spec.orderBy is not null)
                query = query.OrderBy(spec.orderBy);
            if (spec.orderByDesc is not null)
                query = query.OrderByDescending(spec.orderByDesc);

            if (spec.IsPaginationEnabled)
                query = query.Skip(spec.Skip).Take(spec.Take);

            return query;
		}
    }
}
