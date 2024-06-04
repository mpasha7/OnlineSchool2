namespace OnlineSchool2.Models
{
    public class EFSchoolRepository : ISchoolRepository
    {
        private SchoolContext db;

        public IQueryable<Course> Courses => db.Courses;
        public IQueryable<Lesson> Lessons => db.Lessons;
        public IQueryable<StudentOfCourse> Students => db.Students;

        public EFSchoolRepository(SchoolContext ctx)
        {
            this.db = ctx;
        }

        public async void SaveCourse(Course c)
        {
            await db.SaveChangesAsync();
        }
        public async void CreateCourse(Course c)
        {
            await db.AddAsync(c);
            await db.SaveChangesAsync();
        }
        public async void DeleteCourse(Course c)
        {
            db.Remove(c);
            await db.SaveChangesAsync();
        }

        public async void SaveLesson(Lesson l)
        {
            await db.SaveChangesAsync();
        }
        public async void CreateLesson(Lesson l)
        {
            await db.AddAsync(l);
            await db.SaveChangesAsync();
        }
        public async void DeleteLesson(Lesson l)
        {
            db.Remove(l);
            await db.SaveChangesAsync();
        }


        public async void SaveStudentOfCourse(StudentOfCourse s)
        {
            await db.SaveChangesAsync();
        }
        public async void CreateStudentOfCourse(StudentOfCourse s)
        {
            await db.AddAsync(s);
            await db.SaveChangesAsync();
        }
        public async void DeleteStudentOfCourse(StudentOfCourse s)
        {
            db.Remove(s);
            await db.SaveChangesAsync();
        }
    }
}
