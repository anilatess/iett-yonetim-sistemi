using System.Linq.Expressions;
using IETT.Core.DataAccess;
using IETT.Entity.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IETT.DataAccess.Repositories
{
    public class EfEntityRepositoryBase<T, TContext>
        : IEntityRepository<T>
        where T : class, IEntity, new()
        where TContext : DbContext
    {
        protected readonly TContext Context;

        public EfEntityRepositoryBase(TContext context)
        {
            Context = context;
        }

        public async Task<T?> GetAsync(
            Expression<Func<T, bool>> filter)
        {
            return await Context.Set<T>()
                .FirstOrDefaultAsync(filter);
        }

        public async Task<List<T>> GetListAsync(
            Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> query = Context.Set<T>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await Context.Set<T>().AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public void Update(T entity)
        {
            Context.Set<T>().Update(entity);
            Context.SaveChanges();
        }

        public void Delete(T entity)
        {
            Context.Set<T>().Remove(entity);
            Context.SaveChanges();
        }

        public async Task<bool> AnyAsync(
            Expression<Func<T, bool>> filter)
        {
            return await Context.Set<T>()
                .AnyAsync(filter);
        }
    }
}