using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineSchool2.Models;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace OnlineSchool2.Pages.HomeList
{
    public class EditorModel : PageModel
    {
        private SchoolContext db;

        [BindProperty]
        public int CourseId { get; set; }
        [Required]
        [BindProperty]
        public int RelativeId { get; set; }
        [BindProperty]
        public string Choice { get; set; }

        public Course? Course { get; set; }
        public IEnumerable<Course> Courses { get; set; }        

        public EditorModel(SchoolContext ctx)
        {
            this.db = ctx;
            Choice = "Перед";
        }

        public async Task OnGet(int id)
        {
            Course = await db.Courses.FindAsync(id);
            Courses = db.Courses.Where(c => c.Id != id);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var courses = db.Courses.OrderByDescending(c => c.CreatedDate).ToList();
                Course? course = courses.FirstOrDefault(c => c.Id == CourseId);
                DateTime? before;
                DateTime? after;
                if (Choice == "Перед")
                {
                    after = courses.FirstOrDefault(c => c.Id == RelativeId)?.CreatedDate;

                    before = courses.Where(c => c.CreatedDate > after).Any()
                        ? courses.Where(c => c.CreatedDate > after).Min(c => c.CreatedDate)
                        : after.Value.AddDays(1);
                }
                else
                {
                    before = courses.FirstOrDefault(c => c.Id == RelativeId)?.CreatedDate;
                    after = courses.Where(c => c.CreatedDate < before).Any()
                        ? courses.Where(c => c.CreatedDate < before).Max(c => c.CreatedDate)
                        : before.Value.AddDays(-1);
                }

                if (course != null && before != null && after != null)
                {
                    TimeSpan diff = (after.Value - before.Value) / 2;
                    course.CreatedDate = before.Value + diff;
                    db.Courses.Update(course);
                    await db.SaveChangesAsync();
                    return RedirectToPage("List");
                }
                ModelState.AddModelError("", "Ошибка перемещения");
            }
            return Page();
        }
    }
}
