using Microsoft.EntityFrameworkCore;
using Talabat.Core.Entities;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Specifications;
using Talabat.Repository.Data;
using Talabat.Repository.Specifications;

namespace Talabat.Repository
{
	public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
	{
		private readonly StoreContext _storeContext;

		public GenericRepository(StoreContext storeContext)
		{
			_storeContext = storeContext;
		}



		public async Task<IReadOnlyList<T>> GetAllAsync()
		{
			return await _storeContext.Set<T>().ToListAsync();
		}

		public async Task<T?> GetAsync(int id)
		{
			return await _storeContext.Set<T>().FirstOrDefaultAsync(x=>x.Id==id);
		}
		public async Task AddAsync(T entity)
		{
			await _storeContext.Set<T>().AddAsync(entity);
		}
		public void Update(T entity)
		{
			_storeContext.Set<T>().Update(entity);
		}
		public void Delete(T entity)
		{
			entity.IsDeleted = true;
			entity.DeletedDateTime = DateTime.UtcNow;

			_storeContext.Set<T>().Update(entity);
		}

		public void Restore(T entity)
		{
			entity.IsDeleted = false;
			entity.DeletedDateTime = DateTime.UtcNow;

			_storeContext.Set<T>().Update(entity);
		}

		public async Task<IReadOnlyList<T>> GetDeletedAsync()
		{
			return await _storeContext.Set<T>().IgnoreQueryFilters().Where(x => x.IsDeleted).ToListAsync();
		}

		public async Task<T?> GetEntityWithSpecAsync(ISpecifications<T> spec)
		{
			var query = SpecificationsEvaluator<T>.GetQuery(_storeContext.Set<T>().AsQueryable(), spec);
			return await query.FirstOrDefaultAsync();
		}

		public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecifications<T> spec)
		{
			var query = SpecificationsEvaluator<T>.GetQuery(_storeContext.Set<T>().AsQueryable(), spec);
			return await query.ToListAsync();
		}

		public Task<int> GetCountAsync(ISpecifications<T> spec)
		{
			return SpecificationsEvaluator<T>.GetQuery(_storeContext.Set<T>().AsQueryable(),spec).CountAsync();
		}
	}
}
