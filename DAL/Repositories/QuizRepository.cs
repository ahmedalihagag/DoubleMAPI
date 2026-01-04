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
    public class QuizRepository : Repository<Quiz>, IQuizRepository
    {
        public QuizRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Quiz?> GetQuizWithQuestionsAsync(int quizId)
        {
            try
            {
                _logger.Debug("Getting quiz with questions: {QuizId}", quizId);
                return await _dbSet
                    .Include(q => q.Questions.OrderBy(q => q.DisplayOrder))
                        .ThenInclude(q => q.Options.OrderBy(o => o.DisplayOrder))
                    .FirstOrDefaultAsync(q => q.Id == quizId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting quiz with questions: {QuizId}", quizId);
                throw;
            }
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesByCourseAsync(int courseId)
        {
            try
            {
                _logger.Debug("Getting quizzes for course: {CourseId}", courseId);
                return await _dbSet
                    .Where(q => q.CourseId == courseId)
                    .OrderBy(q => q.Title)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting quizzes for course: {CourseId}", courseId);
                throw;
            }
        }
    }
}
