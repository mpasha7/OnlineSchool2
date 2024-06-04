using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace OnlineSchool2.Models
{
    public static class SeedData
    {
        private static string lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua";
        private static string lorem1 = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        private static string lorem2 = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem";
        private static string videoLink = @"https://vk.com/video_ext.php?oid=54023064&id=456239088&hd=2&hash=ec3da24f3c3555ea";

        public static void EnsurePopulated(IApplicationBuilder app)
        {
            EnsurePopulatedAsync(app).Wait();
        }

        public static async Task EnsurePopulatedAsync(IApplicationBuilder app)
        {
            SchoolContext db = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<SchoolContext>();

            if (db.Database.GetPendingMigrations().Any())
            {
                db.Database.Migrate();
            }

            if (!db.Courses.Any())
            {
                UserManager<IdentityUser> userManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                string? firstCoachId = (await userManager.FindByNameAsync("Irina"))?.Id;
                string? secondCoachId = (await userManager.FindByNameAsync("Olga"))?.Id;
                string? firstStudentId = (await userManager.FindByNameAsync("Alex"))?.Id;
                string? secondStudentId = (await userManager.FindByNameAsync("Tom"))?.Id;

                db.Courses.AddRange(
                    new Course
                    {
                        Title = "Нейрогимнастика",
                        Description = lorem2,
                        ShortDescription = lorem,
                        PublicDescription = lorem2,
                        PhotoPath = "/photos/Нейрогимнастика.jpg",
                        CoachGuid = secondCoachId,
                        Lessons = new List<Lesson>
                        {
                            new Lesson { Number = 1, Title = "Lesson 3.1", Description = lorem1, VideoLink = videoLink },
                            new Lesson { Number = 2, Title = "Lesson 3.2", Description = lorem2, VideoLink = videoLink },
                            new Lesson { Number = 3, Title = "Lesson 3.3", Description = lorem2, VideoLink = videoLink }
                        },
                        Students = new List<StudentOfCourse>
                        {
                            new StudentOfCourse { StudentGuid = firstStudentId },
                            new StudentOfCourse { StudentGuid = secondStudentId }
                        }
                    },
                    new Course
                    {
                        Title = "Гимнастика на стопы",
                        Description = lorem1,
                        ShortDescription = lorem,
                        PublicDescription = lorem2,
                        PhotoPath = "/photos/Гимнастика на стопы.jpg",
                        CoachGuid = firstCoachId,
                        Lessons = new List<Lesson>
                        {
                            new Lesson { Number = 1, Title = "Lesson 1.1", Description = lorem1, VideoLink = videoLink },
                            new Lesson { Number = 2, Title = "Lesson 1.2", Description = lorem2, VideoLink = videoLink },
                            new Lesson { Number = 3, Title = "Lesson 1.3", Description = lorem2, VideoLink = videoLink }
                        },
                        Students = new List<StudentOfCourse>
                        {
                            new StudentOfCourse { StudentGuid = firstStudentId },
                            new StudentOfCourse { StudentGuid = secondStudentId }
                        }
                    },
                    new Course
                    {
                        Title = "Йога кундалини",
                        Description = lorem2,
                        ShortDescription = lorem,
                        PublicDescription = lorem2,
                        PhotoPath = "/photos/Йога кундалини.jpg",
                        CoachGuid = firstCoachId,
                        Lessons = new List<Lesson>
                        {
                            new Lesson { Number = 1, Title = "Lesson 2.1", Description = lorem1, VideoLink = videoLink },
                            new Lesson { Number = 2, Title = "Lesson 2.2", Description = lorem2, VideoLink = videoLink },
                            new Lesson { Number = 3, Title = "Lesson 2.3", Description = lorem2, VideoLink = videoLink }
                        },
                        Students = new List<StudentOfCourse>
                        {
                            new StudentOfCourse { StudentGuid = firstStudentId }
                        }
                    },
                    new Course
                    {
                        Title = "Гимнастика на шею",
                        Description = lorem1,
                        ShortDescription = lorem,
                        PublicDescription = lorem2,
                        PhotoPath = "/photos/Гимнастика на шею.jpg",
                        CoachGuid = firstCoachId,
                        Lessons = new List<Lesson>
                        {
                            new Lesson { Number = 1, Title = "Lesson 3.1", Description = lorem1, VideoLink = videoLink },
                            new Lesson { Number = 2, Title = "Lesson 3.2", Description = lorem2, VideoLink = videoLink },
                            new Lesson { Number = 3, Title = "Lesson 3.3", Description = lorem2, VideoLink = videoLink }
                        },
                        Students = new List<StudentOfCourse>
                        {
                            new StudentOfCourse { StudentGuid = firstStudentId }
                        }
                    });
                db.SaveChanges();
            }
        }


    }
}
