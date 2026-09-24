using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Specifications;
using BillsSystem.Infrastructure.Data;
using BillsSystem.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace BillsSystem.Infrastructure.Repositories
{
    public abstract class BaseRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        protected BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        protected IQueryable<T> Query => _dbSet.AsQueryable();

        public virtual async Task<T?> GetByIdAsync(int id) =>
            await Query.FirstOrDefaultAsync(e => e.Id == id);

        public virtual async Task<IEnumerable<T>> GetAllAsync() =>
            await Query.AsNoTracking().ToListAsync();

        public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public virtual async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);
        public virtual void Update(T entity) => _dbSet.Update(entity);
        public virtual void Delete(T entity) => _dbSet.Remove(entity);

        public virtual async Task<IEnumerable<T>> ListAsync(ISpecification<T> spec) =>
            await SpecificationEvaluator<T>.GetQuery(Query.AsNoTracking(), spec).ToListAsync();

        public virtual async Task<int> CountAsync(ISpecification<T> spec) =>
            await SpecificationEvaluator<T>.GetQuery(Query.AsNoTracking(), spec, evaluateCriteriaOnly: true).CountAsync();

        public virtual async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec) =>
            await SpecificationEvaluator<T>.GetQuery(Query.AsNoTracking(), spec).FirstOrDefaultAsync();
    }
}
