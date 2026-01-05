using AutoMapper;
using BLL.DTOs.EnrollmentDTOs;
using BLL.DTOs.ParentStudentDTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    // Parent Service
    public class ParentService : IParentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;

        public ParentService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _logger = Log.ForContext<ParentService>();
        }

        public async Task<string> GenerateLinkCodeAsync(string studentId)
        {
            try
            {
                _logger.Information("Generating link code for student: {StudentId}", studentId);

                var code = GenerateRandomCode();
                var expiresAt = DateTime.UtcNow.AddDays(1);

                var linkCode = new ParentLinkCode
                {
                    Code = code,
                    StudentId = studentId,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ParentLinkCodes.AddAsync(linkCode);
                await _unitOfWork.SaveChangesAsync();

                _logger.Information("Link code generated: {Code}", code);
                return code;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error generating link code");
                throw;
            }
        }

        public async Task<bool> LinkParentToStudentAsync(string parentId, string code)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                _logger.Information("Linking parent: {ParentId} with code: {Code}", parentId, code);

                var linkCode = await _unitOfWork.ParentLinkCodes.FindAsync(lc =>
                    lc.Code == code && !lc.IsUsed && lc.ExpiresAt > DateTime.UtcNow);

                if (linkCode == null)
                {
                    _logger.Warning("Invalid or expired link code: {Code}", code);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                var isLinked = await _unitOfWork.ParentStudents
                    .IsLinkedAsync(parentId, linkCode.StudentId);

                if (isLinked)
                {
                    _logger.Warning("Parent already linked to student");
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                var parentStudent = new ParentStudent
                {
                    ParentId = parentId,
                    StudentId = linkCode.StudentId,
                    LinkedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.ParentStudents.AddAsync(parentStudent);

                linkCode.IsUsed = true;
                linkCode.UsedAt = DateTime.UtcNow;
                _unitOfWork.ParentLinkCodes.Update(linkCode);

                await _unitOfWork.CommitTransactionAsync();

                _logger.Information("Parent linked successfully");
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.Error(ex, "Error linking parent to student");
                throw;
            }
        }

        public async Task<List<StudentInfoDto>> GetLinkedStudentsAsync(string parentId)
        {
            try
            {
                _logger.Debug("Getting linked students for parent: {ParentId}", parentId);
                var links = await _unitOfWork.ParentStudents.GetStudentsByParentAsync(parentId);

                return links.Select(l => new StudentInfoDto
                {
                    Id = l.Student.Id,
                    FullName = l.Student.FullName ?? "",
                    Email = l.Student.Email ?? "",
                    LinkedAt = l.LinkedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting linked students");
                throw;
            }
        }

        public async Task<List<CourseProgressDto>> GetStudentProgressAsync(string parentId, string studentId)
        {
            try
            {
                _logger.Debug("Getting student progress for parent view");

                var isLinked = await _unitOfWork.ParentStudents.IsLinkedAsync(parentId, studentId);
                if (!isLinked)
                {
                    _logger.Warning("Parent not linked to student");
                    throw new UnauthorizedAccessException("Not authorized to view this student's progress");
                }

                var progressService = new ProgressService(_unitOfWork, _mapper);
                return await progressService.GetStudentProgressAsync(studentId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting student progress for parent");
                throw;
            }
        }

        private string GenerateRandomCode()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }
    }
}
