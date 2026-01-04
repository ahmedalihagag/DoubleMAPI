using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class ParentStudentRepository : Repository<ParentStudent>, IParentStudentRepository
    {
        public ParentStudentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ParentStudent>> GetStudentsByParentAsync(string parentId)
        {
            try
            {
                _logger.Debug("Getting students for parent: {ParentId}", parentId);
                return await _dbSet
                    .Where(ps => ps.ParentId == parentId && ps.IsActive)
                    .Include(ps => ps.Student)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting students for parent: {ParentId}", parentId);
                throw;
            }
        }

        public async Task<IEnumerable<ParentStudent>> GetParentsByStudentAsync(string studentId)
        {
            try
            {
                _logger.Debug("Getting parents for student: {StudentId}", studentId);
                return await _dbSet
                    .Where(ps => ps.StudentId == studentId && ps.IsActive)
                    .Include(ps => ps.Parent)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting parents for student: {StudentId}", studentId);
                throw;
            }
        }

        public async Task<bool> IsLinkedAsync(string parentId, string studentId)
        {
            try
            {
                return await _dbSet.AnyAsync(ps =>
                    ps.ParentId == parentId && ps.StudentId == studentId && ps.IsActive);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking parent-student link");
                throw;
            }
        }
    }
}
