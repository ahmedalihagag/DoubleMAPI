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
    // Parent Service
    public class ParentService : IParentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<ParentService> _logger;

        public ParentService(IUnitOfWork uow, IMapper mapper, IEmailService emailService, ILogger<ParentService> logger)
        {
            _unitOfWork = uow;
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
            await _unitOfWork.ParentLinkCodes.AddAsync(linkCode);
            await _unitOfWork.SaveChangesAsync();
            return code;
        }

        public async Task<bool> LinkParentToStudentAsync(string parentId, string code)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var link = await _unitOfWork.ParentLinkCodes
                    .Query()
                    .FirstOrDefaultAsync(lc => lc.Code == code && !lc.IsUsed && lc.ExpiresAt > DateTime.UtcNow);

                if (link == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                var isLinked = await _unitOfWork.ParentStudents.IsLinkedAsync(parentId, link.StudentId);
                if (isLinked)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                await _unitOfWork.ParentStudents.AddAsync(new ParentStudent
                {
                    ParentId = parentId,
                    StudentId = link.StudentId,
                    LinkedAt = DateTime.UtcNow,
                    IsActive = true
                });

                link.IsUsed = true;
                link.UsedAt = DateTime.UtcNow;
                _unitOfWork.ParentLinkCodes.Update(link);

                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<StudentInfoDto>> GetLinkedStudentsAsync(string parentId)
        {
            var links = await _unitOfWork.ParentStudents
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
    }
}

