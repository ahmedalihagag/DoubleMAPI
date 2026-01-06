using DAL.Data;
using DAL.Interfaces;
using DAL.Pagination;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger _logger;

        public Repository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
            _logger = Log.ForContext<Repository<T>>();
        }

        // --------------------
        // GET
        // --------------------
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                _logger.Debug("Getting {EntityType} by Id: {Id}", typeof(T).Name, id);
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting {EntityType} by Id: {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<T?> GetByIdAsync(string id)
        {
            try
            {
                _logger.Debug("Getting {EntityType} by Id: {Id}", typeof(T).Name, id);
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting {EntityType} by Id: {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                _logger.Debug("Getting all {EntityType}", typeof(T).Name);
                return await _dbSet.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting all {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<T?> FindAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.Debug("Finding {EntityType} with predicate", typeof(T).Name);
                return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error finding {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.Debug("Finding all {EntityType} with predicate", typeof(T).Name);
                return await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error finding all {EntityType}", typeof(T).Name);
                throw;
            }
        }

        // --------------------
        // PAGINATION
        // --------------------
        public virtual async Task<PagedResult<T>> GetPagedAsync(PaginationParams paginationParams)
        {
            return await GetPagedAsync(paginationParams, null);
        }

        public virtual async Task<PagedResult<T>> GetPagedAsync(
            PaginationParams paginationParams,
            Expression<Func<T, bool>>? filter = null,
            params Expression<Func<T, object>>[] includes)
        {
            if (paginationParams == null) throw new ArgumentNullException(nameof(paginationParams));

            try
            {
                IQueryable<T> query = _dbSet.AsNoTracking();

                if (includes != null && includes.Length > 0)
                {
                    foreach (var include in includes)
                        query = query.Include(include);
                }

                if (filter != null)
                    query = query.Where(filter);

                if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
                    query = ApplyOrdering(query, paginationParams.SortBy, paginationParams.SortDescending);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToListAsync();

                var metadata = new PaginationMetadata(
                    paginationParams.PageNumber,
                    paginationParams.PageSize,
                    totalCount
                );

                _logger.Information("Retrieved {Count} {EntityType} items (Page {Page}/{TotalPages})",
                    items.Count, typeof(T).Name, metadata.CurrentPage, metadata.TotalPages);

                return new PagedResult<T>(items, metadata);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting paged {EntityType}", typeof(T).Name);
                throw;
            }
        }

        // --------------------
        // COUNT & EXISTS
        // --------------------
        public virtual async Task<int> CountAsync() => await _dbSet.CountAsync();
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate) => await _dbSet.CountAsync(predicate);
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AnyAsync(predicate);

        // --------------------
        // MODIFICATIONS
        // --------------------
        public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public virtual async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);

        public virtual void Update(T entity) => _dbSet.Update(entity);
        public virtual void UpdateRange(IEnumerable<T> entities) => _dbSet.UpdateRange(entities);

        public virtual void Delete(T entity) => _dbSet.Remove(entity);
        public virtual void DeleteRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

        // --------------------
        // QUERY BUILDING
        // --------------------
        public virtual IQueryable<T> Include(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
                query = query.Include(include);
            return query;
        }

        public virtual IQueryable<T> Query() => _dbSet.AsNoTracking();
        public virtual IQueryable<T> AsQueryable() => _dbSet.AsQueryable();

        // --------------------
        // HELPER: ORDERING
        // --------------------
        private static IQueryable<T> ApplyOrdering(IQueryable<T> query, string sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy)) return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression propertyAccess = parameter;

            foreach (var member in sortBy.Split('.'))
                propertyAccess = Expression.PropertyOrField(propertyAccess, member);

            var propertyType = propertyAccess.Type;
            var lambda = Expression.Lambda(propertyAccess, parameter);

            var methodName = descending ? "OrderByDescending" : "OrderBy";

            var method = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), propertyType);

            return (IQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
        }
    }
}
