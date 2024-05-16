using System.ComponentModel.DataAnnotations;

namespace OnlineSchool2.Models
{
    public class StudentOfCourse
    {
        [Key]
        public int Id { get; set; }

        public string? StudentGuid { get; set; }

        public virtual Course? Course { get; set; }

        public StudentOfCourse()
        {
            
        }
    }
}
