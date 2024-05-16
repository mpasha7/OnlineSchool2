using System.ComponentModel.DataAnnotations;

namespace OnlineSchool2.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Введите название курса")]
        public string? Title { get; set; }

        [StringLength(10000, MinimumLength = 10, ErrorMessage = "Введите описание (не менее 10 символов)")]
        public string? Description { get; set; }

        public string? PhotoPath { get; set; }

        public string? CoachGuid { get; set; }////////////////

        public virtual IEnumerable<Lesson> Lessons { get; set; }

        public virtual IEnumerable<StudentOfCourse> Students { get; set; }////////////////

        public Course()
        {
            Lessons = new List<Lesson>();
            Students = new List<StudentOfCourse>();////////////////
        }
    }
}
