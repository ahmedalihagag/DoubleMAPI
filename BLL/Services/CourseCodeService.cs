using AutoMapper;
using BLL.DTOs.CourseDTOs;
using BLL.Interfaces;
using DAL.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class CourseCodeService : ICourseCodeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public CourseCodeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = Log.ForContext<CourseCodeService>();
        }

        public async Task<CourseCodeDto> GenerateCodeAsync(CreateCourseCodeDto dto)
        {
            var entity = await _unitOfWork.CourseCodes.GenerateCodeAsync(
                dto.CourseId, dto.IssuedBy, dto.ExpiresAt);

            await _unitOfWork.SaveChangesAsync();

            _logger.Information("Generated CourseCode {Code} for CourseId {CourseId}", entity.Code, dto.CourseId);

            return _mapper.Map<CourseCodeDto>(entity);
        }

        public async Task<bool> DisableCodeAsync(string code)
        {
            var result = await _unitOfWork.CourseCodes.DisableCodeAsync(code);
            if (result)
                _logger.Information("Disabled CourseCode {Code}", code);
            return result;
        }

        public async Task<bool> EnableCodeAsync(string code)
        {
            var result = await _unitOfWork.CourseCodes.EnableCodeAsync(code);
            if (result)
                _logger.Information("Enabled CourseCode {Code}", code);
            return result;
        }

        public async Task<bool> UseCodeAsync(string code, string studentId)
        {
            var entity = await _unitOfWork.CourseCodes.GetByCodeAsync(code);
            if (entity == null || entity.IsUsed || !entity.IsActive || entity.ExpiresAt < DateTime.UtcNow)
                return false;

            entity.IsUsed = true;
            entity.UsedAt = DateTime.UtcNow;
            entity.UsedBy = studentId;

            _unitOfWork.CourseCodes.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.Information("CourseCode {Code} used by student {StudentId}", code, studentId);
            return true;
        }

        public async Task<CourseCodeDto?> GetByCodeAsync(string code)
        {
            var entity = await _unitOfWork.CourseCodes.GetByCodeAsync(code);
            return entity == null ? null : _mapper.Map<CourseCodeDto>(entity);
        }

        public async Task<IEnumerable<CourseCodeDto>> GetActiveCodesByCourseAsync(int courseId)
        {
            var entities = await _unitOfWork.CourseCodes.GetActiveCodesByCourseIdAsync(courseId);
            return entities.Select(c => _mapper.Map<CourseCodeDto>(c));
        }

        public async Task<bool> UpdateCodeAsync(string code, UpdateCourseCodeDto dto)
        {
            var entity = await _unitOfWork.CourseCodes.GetByCodeAsync(code);
            if (entity == null) return false;

            entity.IsActive = dto.IsActive;
            if (dto.ExpiresAt.HasValue)
                entity.ExpiresAt = dto.ExpiresAt.Value;

            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.CourseCodes.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.Information("Updated CourseCode {Code}: IsActive={IsActive}, ExpiresAt={ExpiresAt}",
                code, dto.IsActive, dto.ExpiresAt);

            return true;
        }
    }
}
