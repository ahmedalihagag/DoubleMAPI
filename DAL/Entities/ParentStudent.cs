using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class ParentStudent
    {
        public int Id { get; set; }

        [Required]
        public string ParentId { get; set; }
        public virtual ApplicationUser Parent { get; set; }

        [Required]
        public string StudentId { get; set; }
        public virtual ApplicationUser Student { get; set; }

        public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
