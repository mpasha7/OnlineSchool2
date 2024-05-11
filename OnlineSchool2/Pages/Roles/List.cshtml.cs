using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnlineSchool2.Pages.Roles
{
    public class ListModel : AdminPageModel
    {
        private UserManager<IdentityUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        public IEnumerable<IdentityRole> Roles { get; set; }

        public ListModel(UserManager<IdentityUser> usrMgr, RoleManager<IdentityRole> roleMgr)
        {
            this.userManager = usrMgr;
            this.roleManager = roleMgr;
        }

        public void OnGet()
        {
            Roles = roleManager.Roles;
        }

        public async Task<string> GetMembersString(string role)
        {
            IEnumerable<IdentityUser> users = await userManager.GetUsersInRoleAsync(role);
            string result = users.Count() == 0
                ? "Нет представителей"
                : string.Join(", ", users.Take(3).Select(u => u.UserName).ToArray());
            return users.Count() > 3
                ? $"{result} и др."
                : result;
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            await roleManager.DeleteAsync(role);
            return RedirectToPage();
        }
    }
}
