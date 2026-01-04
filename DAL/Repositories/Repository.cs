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
            _dbSet = context.Set<T>();
            _logger = Log.ForContext<Repository<T>>();
        }

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
                return await _dbSet.ToListAsync();
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
                return await _dbSet.FirstOrDefaultAsync(predicate);
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
                return await _dbSet.Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error finding all {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<PagedResult<T>> GetPagedAsync(PaginationParams paginationParams)
        {
            if (paginationParams is null)
                throw new ArgumentNullException(nameof(paginationParams));

            try
            {
                _logger.Debug("Getting paged {EntityType} - Page: {Page}, Size: {Size}",
                    typeof(T).Name, paginationParams.PageNumber, paginationParams.PageSize);

                var query = _dbSet.AsQueryable();

                if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
                {
                    query = ApplyOrdering(query, paginationParams.SortBy, paginationParams.SortDescending);
                }

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

        public virtual async Task<PagedResult<T>> GetPagedAsync(
            PaginationParams paginationParams,
            Expression<Func<T, bool>>? filter = null,
            params Expression<Func<T, object>>[] includes)
        {
            if (paginationParams is null)
                throw new ArgumentNullException(nameof(paginationParams));

            try
            {
                _logger.Debug("Getting filtered paged {EntityType} - Page: {Page}, Size: {Size}, Includes: {IncludeCount}",
                    typeof(T).Name, paginationParams.PageNumber, paginationParams.PageSize, includes?.Length ?? 0);

                IQueryable<T> query = _dbSet;

                if (includes != null && includes.Length > 0)
                {
                    foreach (var include in includes)
                    {
                        query = query.Include(include);
                    }
                }

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
                {
                    query = ApplyOrdering(query, paginationParams.SortBy, paginationParams.SortDescending);
                }

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

                _logger.Information("Retrieved {Count} filtered {EntityType} items (Page {Page}/{TotalPages})",
                    items.Count, typeof(T).Name, metadata.CurrentPage, metadata.TotalPages);

                return new PagedResult<T>(items, metadata);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting filtered paged {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<int> CountAsync()
        {
            try
            {
                return await _dbSet.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error counting {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.CountAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error counting {EntityType} with predicate", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.AnyAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking existence of {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task AddAsync(T entity)
        {
            try
            {
                _logger.Debug("Adding {EntityType}", typeof(T).Name);
                await _dbSet.AddAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                _logger.Debug("Adding {Count} {EntityType} entities", entities?.Count() ?? 0, typeof(T).Name);
                await _dbSet.AddRangeAsync(entities);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding range of {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual void Update(T entity)
        {
            try
            {
                _logger.Debug("Updating {EntityType}", typeof(T).Name);
                _dbSet.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            try
            {
                _logger.Debug("Updating {Count} {EntityType} entities", entities?.Count() ?? 0, typeof(T).Name);
                _dbSet.UpdateRange(entities);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating range of {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual void Delete(T entity)
        {
            try
            {
                _logger.Debug("Deleting {EntityType}", typeof(T).Name);
                _dbSet.Remove(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual void DeleteRange(IEnumerable<T> entities)
        {
            try
            {
                _logger.Debug("Deleting {Count} {EntityType} entities", entities?.Count() ?? 0, typeof(T).Name);
                _dbSet.RemoveRange(entities);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting range of {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual IQueryable<T> Include(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query;
        }

        public virtual IQueryable<T> AsQueryable()
        {
            return _dbSet.AsQueryable();
        }

        // Helper: apply ordering by property name (supports nested properties like "Parent.Name")
        private static IQueryable<T> ApplyOrdering(IQueryable<T> query, string sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query;

            // build expression: x => x.Prop1.Prop2
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression propertyAccess = parameter;

            foreach (var member in sortBy.Split('.'))
            {
                propertyAccess = Expression.PropertyOrField(propertyAccess, member);
            }

            var propertyType = propertyAccess.Type;
            var lambda = Expression.Lambda(propertyAccess, parameter);

            var methodName = descending ? "OrderByDescending" : "OrderBy";

            var queryableType = typeof(Queryable);
            var method = queryableType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
                .Single()
                .MakeGenericMethod(typeof(T), propertyType);

            var orderedQuery = (IQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
            return orderedQuery;
        }
    }
}
