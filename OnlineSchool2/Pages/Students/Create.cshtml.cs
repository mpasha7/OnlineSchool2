using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineSchool2.Models;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace OnlineSchool2.Pages.Students
{
    public class CreateModel : CoachPageModel
    {
        private UserManager<IdentityUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        private SchoolContext db;

        [BindProperty]
        [Required]
        public string UserName { get; set; }

        [BindProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [BindProperty]
        [Required]
        public string Password { get; set; }

        [BindProperty]
        public int? CourseId { get; set; }
        public IEnumerable<Course> Courses { get; set; }

        public CreateModel(UserManager<IdentityUser> usrMgr, RoleManager<IdentityRole> roleMgr, SchoolContext ctx)
        {
            this.userManager = usrMgr;
            this.roleManager = roleMgr;
            this.db = ctx;
        }

        public async Task OnGetAsync()
        {
            string? userId = (await userManager.FindByNameAsync(User.Identity.Name))?.Id;
            Courses = db.Courses.Where(c => c.CoachGuid == userId).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                IdentityUser user = new IdentityUser
                {
                    UserName = UserName,
                    Email = Email
                };
                IdentityResult result = await userManager.CreateAsync(user, Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Student");
                }
                if (result.Succeeded)
                {
                    if (CourseId > 0)
                    {
                        Course? course = await db.Courses.FindAsync(CourseId);
                        StudentOfCourse studentOfCourse = new StudentOfCourse
                        {
                            Id = default,
                            Course = course,
                            StudentGuid = user.Id
                        };
                        await db.Students.AddAsync(studentOfCourse);
                        await db.SaveChangesAsync();
                    }
                    return RedirectToPage("List");
                }
                foreach (IdentityError err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
            return Page();
        }
    }
}
