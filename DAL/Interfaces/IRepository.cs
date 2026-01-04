using DAL.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Basic CRUD
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate);

        // Pagination Methods
        Task<PagedResult<T>> GetPagedAsync(PaginationParams paginationParams);
        Task<PagedResult<T>> GetPagedAsync(
            PaginationParams paginationParams,
            Expression<Func<T, bool>>? filter = null,
            params Expression<Func<T, object>>[] includes);

        // Count operations
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        // Modifications
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);

        // Query building
        IQueryable<T> Include(params Expression<Func<T, object>>[] includes);
        IQueryable<T> AsQueryable();
    }
}