using Microsoft.EntityFrameworkCore;
using SkillAllocationTracker.Application.Interfaces;
using SkillAllocationTracker.Infrastructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SkillAllocationTracker.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<T> _set;
        public GenericRepository(AppDbContext db)
        {
            _db = db;
            _set = db.Set<T>();
        }

        public async Task AddAsync(T entity) => await _set.AddAsync(entity);
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => await _set.Where(predicate).ToListAsync();
        public async Task<IEnumerable<T>> GetAllAsync() => await _set.ToListAsync();
        public async Task<T?> GetByIdAsync(Guid id) => await _set.FindAsync(id);
        public void Remove(T entity) => _set.Remove(entity);
        public void Update(T entity) => _set.Update(entity);
    }
}