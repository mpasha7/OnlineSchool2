using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace OnlineSchool2.Pages.Roles
{
    public class EditorModel : AdminPageModel
    {
        private UserManager<IdentityUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        public IdentityRole Role { get; set; }
        public List<string> MembersOrNot { get; set; } = new List<string> { "Представители", "НЕ представители"};
        public string Choice { get; set; }
        public string SearchString { get; set; }

        public EditorModel(UserManager<IdentityUser> usrMgr, RoleManager<IdentityRole> roleMgr)
        {
            this.userManager = usrMgr;
            this.roleManager = roleMgr;
        }

        public async Task<IEnumerable<IdentityUser>> Members() =>
            await userManager.GetUsersInRoleAsync(Role.Name);

        public async Task<IEnumerable<IdentityUser>> NonMembers() =>
            userManager.Users.ToList().Except(await Members());

        public async Task OnGetAsync(string id, string choice = "Представители", string searchstring = "")
        {
            Role = await roleManager.FindByIdAsync(id);
            Choice = choice;
            SearchString = searchstring;
        }

        public async Task<IActionResult> OnPostAsync(string userid, string rolename)
        {
            Role = await roleManager.FindByNameAsync(rolename);
            IdentityUser user = await userManager.FindByIdAsync(userid);
            IdentityResult result;
            if (await userManager.IsInRoleAsync(user, rolename))
            {
                result = await userManager.RemoveFromRoleAsync(user, rolename);
            }
            else
            {
                result = await userManager.AddToRoleAsync(user, rolename);
            }
            if (result.Succeeded)
            {
                return RedirectToPage();
            }
            else
            {
                foreach (IdentityError err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostChoiceAsync(string searchstring, string choice)
        {
            return RedirectToPage("Editor", new { searchstring = searchstring, choice = choice });
        }

        //public async Task<IActionResult> OnPostSearchAsync(string searchstring)
        //{
        //    return RedirectToPage("Editor", new { searchstring = searchstring });
        //}
    }
}
