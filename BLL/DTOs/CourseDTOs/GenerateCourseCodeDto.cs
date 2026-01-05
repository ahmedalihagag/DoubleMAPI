using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.CourseDTOs
{
    public class GenerateCourseCodeDto
    {
        public int CourseId { get; set; }
        public int ExpiryDays { get; set; } = 30;
    }
}
