using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositories
        ICourseRepository Courses { get; }
        ICourseEnrollmentRepository CourseEnrollments { get; }
        IRepository<Section> Sections { get; }
        IRepository<Lesson> Lessons { get; }
        ILessonProgressRepository LessonProgresses { get; }
        ICourseProgressRepository CourseProgresses { get; }
        IQuizRepository Quizzes { get; }
        IRepository<Question> Questions { get; }
        IRepository<Option> Options { get; }
        IQuizAttemptRepository QuizAttempts { get; }
        IRepository<StudentAnswer> StudentAnswers { get; }
        INotificationRepository Notifications { get; }
        IParentStudentRepository ParentStudents { get; }
        IRepository<ParentLinkCode> ParentLinkCodes { get; }
        IRepository<CourseCode> CourseCodes { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IBlacklistRepository Blacklists { get; }
        IEmailLogRepository EmailLogs { get; }
        IFileMetadataRepository FileMetadatas { get; }

        // Unit of Work operations
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
