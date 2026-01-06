using AutoMapper;
using BLL.DTOs.CourseDTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseService> _logger;

        public CourseService(IUnitOfWork uow, IMapper mapper, ILogger<CourseService> logger)
        {
            _unitOfWork = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CourseDto> CreateCourseAsync(CreateCourseDto dto)
        {
            var course = _mapper.Map<Course>(dto);
            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CourseDto>(course);
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int courseId)
        {
            var course = await _unitOfWork.Courses
                .Query()
                .AsNoTracking()
                .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);

            return course == null ? null : _mapper.Map<CourseDto>(course);
        }

        public async Task<List<CourseDto>> GetAllCoursesAsync()
        {
            var courses = await _unitOfWork.Courses
                .Query()
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .Include(c => c.Sections)
                .ToListAsync();

            return _mapper.Map<List<CourseDto>>(courses);
        }

        public async Task<bool> UpdateCourseAsync(int courseId, CreateCourseDto dto)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null) return false;

            course.Title = dto.Title;
            course.Description = dto.Description;
            course.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCourseAsync(int courseId)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null) return false;

            course.IsDeleted = true;
            course.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateCourseAccessCodeAsync(int courseId, string adminId)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null) throw new Exception("Course not found");

            var code = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            var accessCode = new CourseAccessCode
            {
                Code = code,
                CourseId = courseId,
                CreatedBy = adminId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _unitOfWork.CourseAccessCodes.AddAsync(accessCode);
            await _unitOfWork.SaveChangesAsync();

            return code;
        }
    }
}
