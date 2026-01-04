using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private IDbContextTransaction? _transaction;

        // Lazy-loaded repositories
        private ICourseRepository? _courses;
        private ICourseEnrollmentRepository? _courseEnrollments;
        private IRepository<Section>? _sections;
        private IRepository<Lesson>? _lessons;
        private ILessonProgressRepository? _lessonProgresses;
        private ICourseProgressRepository? _courseProgresses;
        private IQuizRepository? _quizzes;
        private IRepository<Question>? _questions;
        private IRepository<Option>? _options;
        private IQuizAttemptRepository? _quizAttempts;
        private IRepository<StudentAnswer>? _studentAnswers;
        private INotificationRepository? _notifications;
        private IParentStudentRepository? _parentStudents;
        private IRepository<ParentLinkCode>? _parentLinkCodes;
        private IRepository<CourseCode>? _courseCodes;
        private IRefreshTokenRepository? _refreshTokens;
        private IBlacklistRepository? _blacklists;
        private IEmailLogRepository? _emailLogs;
        private IFileMetadataRepository? _fileMetadatas;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            _logger = Log.ForContext<UnitOfWork>();
        }

        // Repository Properties with Lazy Initialization
        public ICourseRepository Courses =>
            _courses ??= new CourseRepository(_context);

        public ICourseEnrollmentRepository CourseEnrollments =>
            _courseEnrollments ??= new CourseEnrollmentRepository(_context);

        public IRepository<Section> Sections =>
            _sections ??= new Repository<Section>(_context);

        public IRepository<Lesson> Lessons =>
            _lessons ??= new Repository<Lesson>(_context);

        public ILessonProgressRepository LessonProgresses =>
            _lessonProgresses ??= new LessonProgressRepository(_context);

        public ICourseProgressRepository CourseProgresses =>
            _courseProgresses ??= new CourseProgressRepository(_context);

        public IQuizRepository Quizzes =>
            _quizzes ??= new QuizRepository(_context);

        public IRepository<Question> Questions =>
            _questions ??= new Repository<Question>(_context);

        public IRepository<Option> Options =>
            _options ??= new Repository<Option>(_context);

        public IQuizAttemptRepository QuizAttempts =>
            _quizAttempts ??= new QuizAttemptRepository(_context);

        public IRepository<StudentAnswer> StudentAnswers =>
            _studentAnswers ??= new Repository<StudentAnswer>(_context);

        public INotificationRepository Notifications =>
            _notifications ??= new NotificationRepository(_context);

        public IParentStudentRepository ParentStudents =>
            _parentStudents ??= new ParentStudentRepository(_context);

        public IRepository<ParentLinkCode> ParentLinkCodes =>
            _parentLinkCodes ??= new Repository<ParentLinkCode>(_context);

        public IRepository<CourseCode> CourseCodes =>
            _courseCodes ??= new Repository<CourseCode>(_context);

        public IRefreshTokenRepository RefreshTokens =>
            _refreshTokens ??= new RefreshTokenRepository(_context);

        public IBlacklistRepository Blacklists =>
            _blacklists ??= new BlacklistRepository(_context);

        public IEmailLogRepository EmailLogs =>
            _emailLogs ??= new EmailLogRepository(_context);

        public IFileMetadataRepository FileMetadatas =>
            _fileMetadatas ??= new FileMetadataRepository(_context);

        // Unit of Work Operations
        public async Task<int> SaveChangesAsync()
        {
            try
            {
                _logger.Debug("Saving changes to database");
                var result = await _context.SaveChangesAsync();
                _logger.Information("Successfully saved {Count} changes to database", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error saving changes to database");
                throw;
            }
        }

        public async Task BeginTransactionAsync()
        {
            try
            {
                _logger.Debug("Beginning database transaction");
                _transaction = await _context.Database.BeginTransactionAsync();
                _logger.Information("Database transaction started");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error beginning database transaction");
                throw;
            }
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                _logger.Debug("Committing database transaction");
                await _context.SaveChangesAsync();

                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                    _logger.Information("Database transaction committed successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error committing database transaction");
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    _logger.Warning("Rolling back database transaction");
                    await _transaction.RollbackAsync();
                    await _transaction.DisposeAsync();
                    _transaction = null;
                    _logger.Information("Database transaction rolled back");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error rolling back database transaction");
                throw;
            }
        }

        public void Dispose()
        {
            _logger.Debug("Disposing UnitOfWork");
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
