using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSchool2.Controllers;
using OnlineSchool2.Models;
using System.Reflection;

namespace OnlineSchool2.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_ListsCoursesFromDatabase()
        {
            DbContextOptionsBuilder<SchoolContext> optionsBuilder = new();
            optionsBuilder.UseInMemoryDatabase(MethodBase.GetCurrentMethod().Name);

            using (SchoolContext ctx = new(optionsBuilder.Options))
            {
                ctx.Add(new Course 
                { 
                    Title = "Course1", 
                    Description = "Description1",
                    ShortDescription = "ShortDescription1",
                    PublicDescription = "PublicDescription1",
                    PhotoPath = "PhotoPath1",
                    CoachGuid = "CoachGuid1",
                    BeginQuestionnaire = "BeginQuestionnaire1",
                    EndQuestionnaire = "EndQuestionnaire1",
                });
                ctx.Add(new Course
                {
                    Title = "Course2",
                    Description = "Description2",
                    ShortDescription = "ShortDescription2",
                    PublicDescription = "PublicDescription2",
                    PhotoPath = "PhotoPath2",
                    CoachGuid = "CoachGuid2",
                    BeginQuestionnaire = "BeginQuestionnaire2",
                    EndQuestionnaire = "EndQuestionnaire2",
                });
                ctx.SaveChanges();
            }

            IActionResult result;
            using (SchoolContext ctx = new(optionsBuilder.Options))
            {
                result = new HomeController(ctx).Index();
            }

            var viewResult = Assert.IsType<ViewResult>(result);
            var courses = Assert.IsType<List<Course>>(viewResult.ViewData.Model);
            courses.Sort((a, b) => a.Id.CompareTo(b.Id));
            Assert.NotNull(courses);
            Assert.Equal(2, courses.Count);

            Assert.NotNull(courses[0]);
            Assert.Equal(1, courses[0].Id);
            Assert.Equal(DateTime.Now.Date, courses[0].CreatedDate.Date);
            Assert.Equal("Course1", courses[0].Title);
            Assert.Equal("Description1", courses[0].Description);
            Assert.Equal("ShortDescription1", courses[0].ShortDescription);
            Assert.Equal("PublicDescription1", courses[0].PublicDescription);
            Assert.Equal("PhotoPath1", courses[0].PhotoPath);
            Assert.Equal("CoachGuid1", courses[0].CoachGuid);
            Assert.Equal("BeginQuestionnaire1", courses[0].BeginQuestionnaire);
            Assert.Equal("EndQuestionnaire1", courses[0].EndQuestionnaire);

            Assert.NotNull(courses[1]);
            Assert.Equal(2, courses[1].Id);
            Assert.Equal(DateTime.Now.Date, courses[1].CreatedDate.Date);
            Assert.Equal("Course2", courses[1].Title);
            Assert.Equal("Description2", courses[1].Description);
            Assert.Equal("ShortDescription2", courses[1].ShortDescription);
            Assert.Equal("PublicDescription2", courses[1].PublicDescription);
            Assert.Equal("PhotoPath2", courses[1].PhotoPath);
            Assert.Equal("CoachGuid2", courses[1].CoachGuid);
            Assert.Equal("BeginQuestionnaire2", courses[1].BeginQuestionnaire);
            Assert.Equal("EndQuestionnaire2", courses[1].EndQuestionnaire);
        }

        [Fact]
        public async void Course_GetsCourseFromDatabase()
        {
            DbContextOptionsBuilder<SchoolContext> optionsBuilder = new();
            optionsBuilder.UseInMemoryDatabase(MethodBase.GetCurrentMethod().Name);

            using (SchoolContext ctx = new(optionsBuilder.Options))
            {
                ctx.Add(new Course
                {
                    Title = "Course1",
                    Description = "Description1",
                    ShortDescription = "ShortDescription1",
                    PublicDescription = "PublicDescription1",
                    PhotoPath = "PhotoPath1",
                    CoachGuid = "CoachGuid1",
                    BeginQuestionnaire = "BeginQuestionnaire1",
                    EndQuestionnaire = "EndQuestionnaire1",
                });
                ctx.Add(new Course
                {
                    Title = "Course2",
                    Description = "Description2",
                    ShortDescription = "ShortDescription2",
                    PublicDescription = "PublicDescription2",
                    PhotoPath = "PhotoPath2",
                    CoachGuid = "CoachGuid2",
                    BeginQuestionnaire = "BeginQuestionnaire2",
                    EndQuestionnaire = "EndQuestionnaire2",
                });
                ctx.SaveChanges();
            }

            IActionResult result;
            using (SchoolContext ctx = new(optionsBuilder.Options))
            {
                var controller = new HomeController(ctx);
                result = await controller.Course(2);
            }

            var viewResult = Assert.IsType<ViewResult>(result);
            var course = Assert.IsType<Course>(viewResult.ViewData.Model);
            Assert.NotNull(course);           
            Assert.Equal(2, course.Id);
            Assert.Equal(DateTime.Now.Date, course.CreatedDate.Date);
            Assert.Equal("Course2", course.Title);
            Assert.Equal("Description2", course.Description);
            Assert.Equal("ShortDescription2", course.ShortDescription);
            Assert.Equal("PublicDescription2", course.PublicDescription);
            Assert.Equal("PhotoPath2", course.PhotoPath);
            Assert.Equal("CoachGuid2", course.CoachGuid);
            Assert.Equal("BeginQuestionnaire2", course.BeginQuestionnaire);
            Assert.Equal("EndQuestionnaire2", course.EndQuestionnaire);
        }
    }
}
