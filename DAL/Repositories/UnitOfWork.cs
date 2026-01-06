using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

        // Repositories
        public ICourseRepository Courses { get; private set; }
        public ICourseEnrollmentRepository CourseEnrollments { get; private set; }
        public ISectionRepository Sections { get; private set; }
        public ILessonRepository Lessons { get; private set; }
        public ILessonProgressRepository LessonProgresses { get; private set; }
        public ICourseProgressRepository CourseProgresses { get; private set; }
        public IQuizRepository Quizzes { get; private set; }
        public IQuestionRepository Questions { get; private set; }
        public IOptionRepository Options { get; private set; }
        public IQuizAttemptRepository QuizAttempts { get; private set; }
        public IStudentAnswerRepository StudentAnswers { get; private set; }
        public INotificationRepository Notifications { get; private set; }
        public IParentStudentRepository ParentStudents { get; private set; }
        public IParentLinkCodeRepository ParentLinkCodes { get; private set; }
        public ICourseCodeRepository CourseCodes { get; private set; }
        public IRefreshTokenRepository RefreshTokens { get; private set; }
        public IBlacklistRepository Blacklists { get; private set; }
        public IEmailLogRepository EmailLogs { get; private set; }
        public IFileMetadataRepository FileMetadatas { get; private set; }
        public ICourseAccessCodeRepository CourseAccessCodes { get; private set; }
        public UserManager<ApplicationUser> UserManager { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = Log.ForContext<UnitOfWork>();

            // Initialize repositories
            Courses = new CourseRepository(_context);
            CourseEnrollments = new CourseEnrollmentRepository(_context);
            Sections = new SectionRepository(_context);
            Lessons = new LessonRepository(_context);
            LessonProgresses = new LessonProgressRepository(_context);
            CourseProgresses = new CourseProgressRepository(_context);
            Quizzes = new QuizRepository(_context);
            Questions = new QuestionRepository(_context);
            Options = new OptionRepository(_context);
            QuizAttempts = new QuizAttemptRepository(_context);
            StudentAnswers = new StudentAnswerRepository(_context);
            Notifications = new NotificationRepository(_context);
            ParentStudents = new ParentStudentRepository(_context);
            ParentLinkCodes = new ParentLinkCodeRepository(_context);
            CourseCodes = new CourseCodeRepository(_context);
            RefreshTokens = new RefreshTokenRepository(_context);
            Blacklists = new BlacklistRepository(_context);
            EmailLogs = new EmailLogRepository(_context);
            FileMetadatas = new FileMetadataRepository(_context);
            CourseAccessCodes = new CourseAccessCodeRepository(_context);
            UserManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(_context),
                null, // IOptions<IdentityOptions>
                new PasswordHasher<ApplicationUser>(),
                null, // IEnumerable<IUserValidator<ApplicationUser>>
                null, // IEnumerable<IPasswordValidator<ApplicationUser>>
                null, // ILookupNormalizer
                null, // IdentityErrorDescriber
                null, // IServiceProvider
                null  // ILogger<UserManager<ApplicationUser>>
            );
        }

        // --------------------
        // TRANSACTIONS
        // --------------------
        public async Task BeginTransactionAsync()
        {
            try
            {
                _logger.Debug("Beginning transaction");
                await _context.Database.BeginTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error beginning transaction");
                throw;
            }
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                _logger.Debug("Committing transaction");
                await _context.Database.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error committing transaction");
                throw;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                _logger.Debug("Rolling back transaction");
                await _context.Database.RollbackTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error rolling back transaction");
                throw;
            }
        }

        // --------------------
        // SAVE
        // --------------------
        public async Task<int> SaveChangesAsync()
        {
            try
            {
                _logger.Debug("Saving changes to database");
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error saving changes to database");
                throw;
            }
        }

        public async Task AddRefreshTokenAsync(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        }

        // --------------------
        // DISPOSE
        // --------------------
        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
