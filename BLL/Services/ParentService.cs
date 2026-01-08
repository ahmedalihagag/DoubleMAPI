using AutoMapper;
using BLL.DTOs.EnrollmentDTOs;
using BLL.DTOs.ParentStudentDTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ParentService : IParentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<ParentService> _logger;

        public ParentService(
            IUnitOfWork uow,
            IMapper mapper,
            IEmailService emailService,
            ILogger<ParentService> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<string> GenerateLinkCodeAsync(string studentId)
        {
            var code = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            var linkCode = new ParentLinkCode
            {
                Code = code,
                StudentId = studentId,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow
            };

            await _uow.ParentLinkCodes.AddAsync(linkCode);
            await _uow.SaveChangesAsync();

            return code;
        }

        public async Task<bool> LinkParentToStudentAsync(string parentId, string code)
        {
            await _uow.BeginTransactionAsync();

            try
            {
                var link = await _uow.ParentLinkCodes
                    .Query()
                    .FirstOrDefaultAsync(lc => lc.Code == code && !lc.IsUsed && lc.ExpiresAt > DateTime.UtcNow);

                if (link == null)
                {
                    await _uow.RollbackTransactionAsync();
                    return false;
                }

                var isLinked = await _uow.ParentStudents.IsLinkedAsync(parentId, link.StudentId);
                if (isLinked)
                {
                    await _uow.RollbackTransactionAsync();
                    return false;
                }

                await _uow.ParentStudents.AddAsync(new ParentStudent
                {
                    ParentId = parentId,
                    StudentId = link.StudentId,
                    LinkedAt = DateTime.UtcNow,
                    IsActive = true
                });

                link.IsUsed = true;
                link.UsedAt = DateTime.UtcNow;
                _uow.ParentLinkCodes.Update(link);

                await _uow.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<StudentInfoDto>> GetLinkedStudentsAsync(string parentId)
        {
            var links = await _uow.ParentStudents
                .Query()
                .Include(ps => ps.Student)
                .AsNoTracking()
                .Where(ps => ps.ParentId == parentId && ps.IsActive)
                .ToListAsync();

            return links.Select(l => new StudentInfoDto
            {
                Id = l.Student.Id,
                FullName = l.Student.FullName ?? "",
                Email = l.Student.Email ?? "",
                LinkedAt = l.LinkedAt
            }).ToList();
        }

        // ✅ NEW METHOD: Check if parent is linked to student
        public async Task<bool> IsLinkedAsync(string parentId, string studentId)
        {
            try
            {
                _logger.LogInformation("Checking if parent {ParentId} is linked to student {StudentId}",
                    parentId, studentId);

                var isLinked = await _uow.ParentStudents.IsLinkedAsync(parentId, studentId);

                _logger.LogInformation("Parent {ParentId} link status with student {StudentId}: {IsLinked}",
                    parentId, studentId, isLinked);

                return isLinked;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking parent-student link");
                throw;
            }
        }
    }
}

