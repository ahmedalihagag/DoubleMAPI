using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class CourseAccessCode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = null!;

        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;

        [Required]
        [MaxLength(450)]
        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }

        [MaxLength(450)]
        public string? UsedBy { get; set; }

        public bool IsDisabled { get; set; } = false;

        public DateTime? DisabledAt { get; set; }
    }
}
