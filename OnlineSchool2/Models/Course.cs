using System.ComponentModel.DataAnnotations;

namespace OnlineSchool2.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? PhotoPath { get; set; }
        public virtual IEnumerable<Lesson> Lessons { get; set; }

        public Course()
        {
            Lessons = new List<Lesson>();
        }
    }
}
