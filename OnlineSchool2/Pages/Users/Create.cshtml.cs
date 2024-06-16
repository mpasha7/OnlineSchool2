using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineSchool2.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineSchool2.Pages.Users
{
    [IgnoreAntiforgeryToken]
    public class CreateModel : AdminPageModel
    {
        private UserManager<IdentityUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        [BindProperty]
        [Required(ErrorMessage = "¬ведите им€")]
        public string UserName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "¬ведите Email")]
        [EmailAddress]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "¬ведите пароль")]
        public string Password { get; set; }

        [BindProperty]
        [Compare("Password", ErrorMessage = "ѕароли не совпадают")]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        public string? RoleId { get; set; }
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
                if (!string.IsNullOrWhiteSpace(RoleId) && RoleId != "None" && result.Succeeded)
                {
                    IdentityRole role = await roleManager.FindByIdAsync(RoleId);
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
