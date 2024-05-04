using System.ComponentModel.DataAnnotations;

namespace OnlineSchool2.Models
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? VideoLink { get; set; }
        public virtual Course? Course { get; set; }

        public Lesson() { }
    }
}
