namespace OnlineSchool2.Models
{
    public class LessonsListViewModel
    {
        public IEnumerable<Lesson>? Lessons { get; set; }
        public Course? Course { get; set; }
    }
}
