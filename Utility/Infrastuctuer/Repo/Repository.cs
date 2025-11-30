 
namespace Utility.Infrastuctuer.Repo
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Utility.Appliation.RepoInterface;

    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>where TEntity : class
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public Repository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            // 2. توکن را به عنوان آرگومان دوم به FindAsync پاس بده
            // نکته: ورودی اول FindAsync آرایه‌ای از آبجکت‌هاست (برای کلیدهای ترکیبی)، 
            // پس id را داخل آبجکت می‌گذاریم تا کامپایلر گیج نشود.
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public void Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
        }
        public async Task<bool> RemoveByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            // 1. جستجو با توکن (اگر کنسل شود، همینجا متوقف می‌شود)
            var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                return false;

            // 2. حذف (در حافظه)
            _dbSet.Remove(entity);

            // نکته: اگر متد شما SaveChanges را هم صدا می‌زند، توکن را به آن هم پاس دهید.
            // اما طبق کدهای قبلی شما، ظاهراً SaveChanges یا Commit جداگانه صدا زده می‌شود.

            return true;
        }
        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            // توکن را به متد AnyAsync پاس بدهید
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        public IQueryable<TEntity> Query()
        {
            return _dbSet.AsQueryable();
        }

        public IQueryable<TEntity> QueryBy(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.Where(predicate).AsQueryable();
        }

        public IQueryable<TEntity> Include(params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _dbSet.AsQueryable();
            return includes.Aggregate(query, (current, include) => current.Include(include));
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return true; // حتی اگر هیچ تغییری نکرده باشه
            }
            catch
            {
                return false; // در صورت خطا
            }
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            await _context.Database.CommitTransactionAsync(cancellationToken);
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            await _context.Database.RollbackTransactionAsync(cancellationToken);
        }
        public IQueryable<TEntity> GetAllQueryable()
        {
            return _context.Set<TEntity>().AsQueryable();
        }
        public async Task<List<TEntity>> GetAllByQueryAsync(Expression<Func<TEntity, bool>> predicate,  CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }

        
    }
}
