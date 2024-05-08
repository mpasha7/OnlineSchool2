using Microsoft.EntityFrameworkCore;

namespace OnlineSchool2.Models
{
    public static class SeedData
    {
        private static string lorem1 = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        private static string lorem2 = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem";

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
                        Title = "Гимнастика на стопы", Description = lorem1, PhotoPath = "/photos/Гимнастика на стопы.jpg",
                        Lessons = new List<Lesson> 
                        { 
                            new Lesson { Title = "Lesson 1.1", Description = lorem1, VideoLink = "Link 1.1" },
                            new Lesson { Title = "Lesson 1.2", Description = lorem2, VideoLink = "Link 1.2" }
                        }
                    },
                    new Course
                    {
                        Title = "Йога кундалини", Description = lorem2, PhotoPath = "/photos/Йога кундалини.jpg",
                        Lessons = new List<Lesson>
                        {
                            new Lesson { Title = "Lesson 2.1", Description = lorem1, VideoLink = "Link 2.1" },
                            new Lesson { Title = "Lesson 2.2", Description = lorem2, VideoLink = "Link 2.2" }
                        }
                    },
                    new Course
                    {
                        Title = "Гимнастика на шею", Description = lorem1, PhotoPath = "/photos/Гимнастика на шею.jpg",
                        Lessons = new List<Lesson>
                        {
                            new Lesson { Title = "Lesson 2.1", Description = lorem1, VideoLink = "Link 2.1" },
                            new Lesson { Title = "Lesson 2.2", Description = lorem2, VideoLink = "Link 2.2" }
                        }
                    });
                db.SaveChanges();
            }
        }


    }
}
