using BLL.DTOs.LessonDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ILessonService
    {
        Task<LessonDto> CreateLessonAsync(CreateLessonDto createLessonDto);
        Task<LessonDto?> GetLessonByIdAsync(int lessonId);
        Task<List<LessonDto>> GetLessonsBySectionAsync(int sectionId);
        Task<bool> UpdateLessonAsync(int lessonId, CreateLessonDto updateDto);
        Task<bool> DeleteLessonAsync(int lessonId);
        Task<bool> MarkLessonCompleteAsync(string studentId, int lessonId);
    }
}
