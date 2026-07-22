using System.Linq.Expressions;
using IETT.Entity.Interfaces;

namespace IETT.Core.DataAccess
{
    public interface IEntityRepository<T>
        where T : class, IEntity, new()
    {
        Task<T?> GetAsync(Expression<Func<T, bool>> filter);

        Task<List<T>> GetListAsync(
            Expression<Func<T, bool>>? filter = null);

        Task AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);

        Task<bool> AnyAsync(
            Expression<Func<T, bool>> filter);
    }
}