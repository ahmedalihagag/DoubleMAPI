using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Option
    {
        public int Id { get; set; }

        [Required]
        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Text { get; set; }

        public bool IsCorrect { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int DisplayOrder { get; set; }

        //navigation properties
        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();

    }
}
