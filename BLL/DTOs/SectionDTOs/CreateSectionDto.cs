using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.SectionDTOs
{
    public class CreateSectionDto
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int DisplayOrder { get; set; }
    }
}
