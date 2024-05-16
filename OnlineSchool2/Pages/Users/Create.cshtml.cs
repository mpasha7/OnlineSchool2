using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineSchool2.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineSchool2.Pages.Users
{
    public class CreateModel : AdminPageModel
    {
        private UserManager<IdentityUser> userManager;
        private RoleManager<IdentityRole> roleManager;

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
        public string? RoleName { get; set; }//Id
        public IEnumerable<IdentityRole> Roles { get; set; }

        public CreateModel(UserManager<IdentityUser> usrMgr, RoleManager<IdentityRole> roleMgr)
        {
            this.userManager = usrMgr;
            this.roleManager = roleMgr;
        }

        public async Task OnGetAsync()
        {
            Roles = roleManager.Roles.ToList();
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
                if (!string.IsNullOrWhiteSpace(RoleName) && RoleName != "None" && result.Succeeded)
                {
                    IdentityRole role = await roleManager.FindByIdAsync(RoleName);
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                if (result.Succeeded)
                {
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
