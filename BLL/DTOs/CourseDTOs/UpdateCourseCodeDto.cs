using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.CourseDTOs
{
    public class UpdateCourseCodeDto
    {
        [Required]
        public bool IsActive { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }
}
