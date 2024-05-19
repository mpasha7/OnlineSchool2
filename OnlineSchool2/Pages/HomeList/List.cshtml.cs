using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineSchool2.Models;

namespace OnlineSchool2.Pages.HomeList
{
    public class ListModel : AdminPageModel
    {
        private SchoolContext db;

        public IEnumerable<Course> Courses { get; set; }

        public ListModel(SchoolContext ctx)
        {
            this.db = ctx;
        }

        public void OnGet()
        {
            Courses = db.Courses.OrderByDescending(c => c.CreatedDate).ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            Course? course = await db.Courses.FindAsync(id);
            if (course is not null)
            {
                db.Courses.Remove(course);
                await db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
