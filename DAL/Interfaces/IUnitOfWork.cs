using DAL.Entities;
using Microsoft.AspNetCore.Identity;
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
        ISectionRepository Sections { get; }
        ILessonRepository Lessons { get; }
        ILessonProgressRepository LessonProgresses { get; }
        ICourseProgressRepository CourseProgresses { get; }
        IQuizRepository Quizzes { get; }
        IQuestionRepository Questions { get; }
        IOptionRepository Options { get; }
        IQuizAttemptRepository QuizAttempts { get; }
        IStudentAnswerRepository StudentAnswers { get; }
        INotificationRepository Notifications { get; }
        IParentStudentRepository ParentStudents { get; }
        IParentLinkCodeRepository ParentLinkCodes { get; }
        ICourseCodeRepository CourseCodes { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IBlacklistRepository Blacklists { get; }
        IEmailLogRepository EmailLogs { get; }
        IFileMetadataRepository FileMetadatas { get; }
        ICourseAccessCodeRepository CourseAccessCodes { get; }


        
        //Identity Related
        UserManager<ApplicationUser> UserManager { get; }
        RoleManager<IdentityRole> RoleManager { get; }

        // Unit of Work operations
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task AddRefreshTokenAsync(RefreshToken token);
        Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash);

    }
}
