using System.Collections;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Repositories.Contract;
using Talabat.Repository.Data;

namespace Talabat.Repository
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly StoreContext _context;

		private Hashtable _repositories;
		public UnitOfWork(StoreContext context)
		{
			_context = context;
			_repositories = new Hashtable();
		}
		public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
		{
			var type = typeof(TEntity).Name;
			if (!_repositories.ContainsKey(type))
			{
			    var repository = new GenericRepository<TEntity>(_context);
				_repositories.Add(type, repository);
			}
			return _repositories[type] as IGenericRepository<TEntity>;
		}
		public async Task<int> CompleteAsync()
			=> await _context.SaveChangesAsync();

		public async ValueTask DisposeAsync()
			=> await _context.DisposeAsync();
	}
}
