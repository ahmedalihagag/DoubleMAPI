using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.SectionDTOs
{
    public class CreateSectionDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}
