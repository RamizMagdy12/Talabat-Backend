using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Specifications;

namespace Talabat.Core.Repositories.Contract
{
   public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
		Task<IReadOnlyList<T>> GetDeletedAsync();
		Task AddAsync(T entity);
		void Update(T entity);
		void Delete(T entity); // Soft Delete
		void Restore(T entity);

		// Specification-based queries
		Task<T?> GetEntityWithSpecAsync(ISpecifications<T> spec);
		Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecifications<T> spec);

		// new because Pagination
		Task<int> GetCountAsync(ISpecifications<T> spec);

	}
}
