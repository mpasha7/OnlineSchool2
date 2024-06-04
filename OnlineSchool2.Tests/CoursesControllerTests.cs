using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineSchool2.Controllers;
using OnlineSchool2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineSchool2.Tests
{
    public class CoursesControllerTests
    {
        //[Fact]
        //public async void CanGetCoursesList()
        //{
        //    // ARRANGE
        //    Mock<SchoolContext> mockDb = new Mock<SchoolContext>();
        //    Mock<IWebHostEnvironment> mockEnv = new Mock<IWebHostEnvironment>();
        //    Mock<UserManager<IdentityUser>> mockUserMgr = new Mock<UserManager<IdentityUser>>();

        //    CoursesController target = new CoursesController(mockDb.Object, mockEnv.Object, mockUserMgr.Object);
        //    target.User.Identity.Name = "Irina";

        //    // ACT
        //    ViewResult result = await target.Index() as ViewResult;

        //    // ASSERT

        //}
    }
}
