using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace OnlineSchool2.Pages.Roles
{
    public class CreateModel : AdminPageModel
    {
        private RoleManager<IdentityRole> roleManager;

        [BindProperty]
        [Required]
        public string Name { get; set; }

        public CreateModel(RoleManager<IdentityRole> roleMgr)
        {
            this.roleManager = roleMgr;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                IdentityRole role = new IdentityRole { Name = Name };
                IdentityResult result = await roleManager.CreateAsync(role);
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
