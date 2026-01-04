using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.LessonDTOs;

namespace BLL.DTOs.SectionDTOs
{
    public class SectionDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public List<LessonDto> Lessons { get; set; } = new();
    }
}
