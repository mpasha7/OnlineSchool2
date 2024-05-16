using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineSchool2.Models;
using System.Data;

namespace OnlineSchool2.Pages.Students
{
    public class EditorModel : PageModel
    {
        private UserManager<IdentityUser> userManager;
        private SchoolContext db;

        public IdentityUser? Coach { get; set; }
        public IdentityUser? Student { get; set; }        

        public EditorModel(UserManager<IdentityUser> usrMgr, SchoolContext ctx)
        {
            this.userManager = usrMgr;
            this.db = ctx;
        }

        public IEnumerable<Course> Purchased() =>
            db.Courses.Include(c => c.Students)
            .Where(c => c.CoachGuid == Coach.Id && c.Students.Where(s => s.StudentGuid == Student.Id).Any()).ToList();

        public IEnumerable<Course> NonPurchased() =>
            db.Courses.Where(c => c.CoachGuid == Coach.Id).ToList().Except(Purchased());

        public async Task OnGetAsync(string id)
        {
            Coach = await userManager.FindByNameAsync(User.Identity.Name);
            Student = await userManager.FindByIdAsync(id);
        }

        public async Task<IActionResult> OnPostAsync(int? courseid, string? studentid)
        {
            if (courseid is not null && !string.IsNullOrWhiteSpace(studentid))
            {
                Course? course = await db.Courses.FindAsync(courseid);
                if (course != null)
                {
                    if (await db.Students.AnyAsync(s => s.StudentGuid == studentid && s.Course == course))
                    {
                        StudentOfCourse studentOfCourse = await db.Students.FirstOrDefaultAsync(s => s.StudentGuid == studentid && s.Course == course);
                        db.Students.Remove(studentOfCourse);
                    }
                    else
                    {
                        StudentOfCourse studentOfCourse = new StudentOfCourse
                        {
                            StudentGuid = studentid,
                            Course = course
                        };
                        await db.Students.AddAsync(studentOfCourse);
                    }
                    await db.SaveChangesAsync();
                    return RedirectToPage();
                }
            }
            return Page();
        }
    }
}
