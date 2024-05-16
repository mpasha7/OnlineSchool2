using System.ComponentModel.DataAnnotations;

namespace OnlineSchool2.Models
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }
        
        public int? Number { get; set; }

        [Required(ErrorMessage = "Введите название урока")]
        public string? Title { get; set; }

        [StringLength(10000, MinimumLength = 10, ErrorMessage = "Введите описание (не менее 10 символов)")]
        public string? Description { get; set; }

        public string? VideoLink { get; set; }

        public virtual Course? Course { get; set; }

        public Lesson() 
        { 

        }
    }
}
