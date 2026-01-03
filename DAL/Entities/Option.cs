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

        public int DisplayOrder { get; set; }
    }
}
