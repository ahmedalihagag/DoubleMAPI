using BLL.DTOs.SectionDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.QuizDTOs;

namespace BLL.DTOs.CourseDTOs
{
    public class CourseDetailDto : CourseDto
    {
        public List<SectionDto> Sections { get; set; } = new();
        public List<QuizDto> Quizzes { get; set; } = new();
    }
}
