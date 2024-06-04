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
    public class AccountControllerTests
    {
        [Fact]
        public async void CanGetCoursesList()
        {
            // ARRANGE
            LoginModel loginModel = new LoginModel
            {
                Name = "Irina",
                Password = "Irina123$"
            };
            
            Mock<UserManager<IdentityUser>> mockUserMgr = new Mock<UserManager<IdentityUser>>();
            Mock<SignInManager<IdentityUser>> mockSignInMgr = new Mock<SignInManager<IdentityUser>>();

            AccountController controller = new AccountController(mockUserMgr.Object, mockSignInMgr.Object);

            // ACT
            IActionResult result = await controller.Login(loginModel) as RedirectResult;

            // ASSERT
            //Assert.Equal("/", result.);
            Assert.True(mockSignInMgr.Object.IsSignedIn(controller.User));
        }
    }
}
