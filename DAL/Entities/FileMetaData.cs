using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class FileMetadata
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(300)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(50)]
        public string FileType { get; set; } // Video, Image, PDF

        public long FileSize { get; set; }

        [Required]
        [MaxLength(1000)]
        public string BunnyCdnUrl { get; set; }

        [MaxLength(500)]
        public string? LocalPath { get; set; }

        [Required]
        public string UploadedBy { get; set; }
        public virtual ApplicationUser Uploader { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }
    }
}
