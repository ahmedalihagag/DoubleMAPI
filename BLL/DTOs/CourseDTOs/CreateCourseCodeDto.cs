using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.CourseDTOs
{
    public class CreateCourseCodeDto
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public string IssuedBy { get; set; } = null!;

        [Required]
        public DateTime ExpiresAt { get; set; }
    }
}
