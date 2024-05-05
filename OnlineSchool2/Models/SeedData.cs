using Microsoft.EntityFrameworkCore;

namespace OnlineSchool2.Models
{
    public static class SeedData
    {
        public static void SeedDatabase(IApplicationBuilder app)
        {
            SchoolContext db = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<SchoolContext>();

            if (db.Database.GetPendingMigrations().Any())
            {
                db.Database.Migrate();
            }

            if (!db.Courses.Any())
            {
                db.Courses.AddRange(
                    new Course
                    {
                        Title = "Course 1", Description = "Description for course 1", PhotoPath = "Path 1",
                        Lessons = new List<Lesson> 
                        { 
                            new Lesson { Title = "Lesson 1.1", Description = "Description for lesson 1.1", VideoLink = "Link 1.1" },
                            new Lesson { Title = "Lesson 1.2", Description = "Description for lesson 1.2", VideoLink = "Link 1.2" }
                        }
                    },
                    new Course
                    {
                        Title = "Course 2", Description = "Description for course 2", PhotoPath = "Path 2",
                        Lessons = new List<Lesson>
                        {
                            new Lesson { Title = "Lesson 2.1", Description = "Description for lesson 2.1", VideoLink = "Link 2.1" },
                            new Lesson { Title = "Lesson 2.2", Description = "Description for lesson 2.2", VideoLink = "Link 2.2" }
                        }
                    });
                db.SaveChanges();
            }
        }


    }
}
