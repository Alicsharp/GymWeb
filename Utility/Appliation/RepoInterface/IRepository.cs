using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Appliation.RepoInterface
{
    public interface IRepository<TEntity, TKey> where TEntity : class
    {
        // Basic CRUD Operations
        Task<TEntity> GetByIdAsync(TKey id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        IQueryable<TEntity> QueryBy(Expression<Func<TEntity, bool>> predicate);
        Task<List<TEntity>> GetAllByQueryAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        IQueryable<TEntity> GetAllQueryable(); // جدید - برای کوئری‌های پیچیده
       
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

        // Add Operations
        Task AddAsync(TEntity entity);
        
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        // Update Operations
        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);

        // Remove Operations
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
        Task<bool> RemoveByIdAsync(TKey id);

        // Other Operations
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

        // Query Operations
        IQueryable<TEntity> Query();
       

        // Include Operations
        IQueryable<TEntity> Include(params Expression<Func<TEntity, object>>[] includes);

        // Save Changes
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);


        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
