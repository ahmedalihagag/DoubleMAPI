using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class CourseCode
    {
        [Key]
        [MaxLength(20)]
        public string Code { get; set; } = null!;

        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;

        [Required]
        public string IssuedBy { get; set; } = null!;
        public virtual ApplicationUser Issuer { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;
        public DateTime? UsedAt { get; set; }
        public string? UsedBy { get; set; }

        // New: Admin can disable code without deleting
        public bool IsActive { get; set; } = true;

        // Optional: Soft delete if you want
        public bool IsDeleted { get; set; } = false;

        public DateTime? UpdatedAt { get; set; }
    }
}
