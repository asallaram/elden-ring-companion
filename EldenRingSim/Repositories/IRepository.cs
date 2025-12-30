using System.Linq.Expressions;

namespace EldenRingSim.Repositories
{
  
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        
        Task<T?> GetByIdAsync(string id);
        

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        
        Task AddAsync(T entity);
        
        Task UpdateAsync(T entity);
        
        Task DeleteAsync(string id);
    }
}