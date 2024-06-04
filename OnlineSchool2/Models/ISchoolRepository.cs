namespace OnlineSchool2.Models
{
    public interface ISchoolRepository
    {
        IQueryable<Course> Courses { get; }
        IQueryable<Lesson> Lessons { get; }
        IQueryable<StudentOfCourse> Students { get; }

        void SaveCourse(Course c);
        void CreateCourse(Course c);
        void DeleteCourse(Course c);

        void SaveLesson(Lesson l);
        void CreateLesson(Lesson l);
        void DeleteLesson(Lesson l);


        void SaveStudentOfCourse(StudentOfCourse s);
        void CreateStudentOfCourse(StudentOfCourse s);
        void DeleteStudentOfCourse(StudentOfCourse s);
    }
}
