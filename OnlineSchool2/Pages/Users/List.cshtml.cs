using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnlineSchool2.Pages.Users
{
    public class ListModel : AdminPageModel
    {
        private UserManager<IdentityUser> userManager;

        public IEnumerable<IdentityUser> Users { get; set; }

        public ListModel(UserManager<IdentityUser> usrMgr)
        {
            this.userManager = usrMgr;
        }

        public void OnGet()
        {
            Users = userManager.Users;
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            IdentityUser? user = await userManager.FindByIdAsync(id);
            if (user is not null)
            {
                await userManager.DeleteAsync(user);
            }
            return RedirectToPage();
        }
    }
}
