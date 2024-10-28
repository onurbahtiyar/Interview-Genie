using System.Linq.Expressions;

namespace Backend.Repository;

public interface IRepository<T> where T : class
{
    Task<T> GetAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[] includes);
    Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task SaveChangesAsync();
}
