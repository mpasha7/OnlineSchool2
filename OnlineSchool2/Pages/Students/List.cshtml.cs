using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineSchool2.Models;
using System.Data;

namespace OnlineSchool2.Pages.Students
{
    public class ListModel : CoachPageModel
    {
        private UserManager<IdentityUser> userManager;
        private SchoolContext db;

        public IEnumerable<IdentityUser> Users { get; set; }
        public string Search { get; set; }

        public ListModel(UserManager<IdentityUser> usrMgr, SchoolContext ctx)
        {
            this.userManager = usrMgr;
            this.db = ctx;
        }

        public async Task OnGetAsync(string search = "")
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                Search = search;
                Users = (await userManager.GetUsersInRoleAsync("Student"))
                    .Where(u => u.Email != null && u.Email.ToLower().Contains(search.ToLower()));
            }
            else
            {
                string? userId = (await userManager.FindByNameAsync(User.Identity.Name))?.Id;
                var studentsIds = db.Students.Include(s => s.Course).Where(s => s.Course.CoachGuid == userId)
                    .Select(s => s.StudentGuid).Distinct().ToList();
                Users = (await userManager.GetUsersInRoleAsync("Student")).Where(u => studentsIds.Contains(u.Id)).ToList();
            }
        }

        public IActionResult OnPostAsync(string search)
        {
            return RedirectToPage("List", new { search = search });
        }
    }
}
