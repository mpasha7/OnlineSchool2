using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace OnlineSchool2.Pages.Users
{
    public class EditorModel : AdminPageModel
    {
        private UserManager<IdentityUser> userManager;

        [BindProperty]
        [Required]
        public string Id { get; set; }

        [BindProperty]
        [Required]
        public string UserName { get; set; }

        [BindProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [BindProperty]
        public string? Password { get; set; }
        [BindProperty]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string? ConfirmPassword { get; set; }

        public EditorModel(UserManager<IdentityUser> usrMgr)
        {
            this.userManager = usrMgr;
        }

        public async Task OnGetAsync(string id)
        {
            IdentityUser user = await userManager.FindByIdAsync(id);
            Id = user.Id;
            UserName = user.UserName;
            Email = user.Email;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                IdentityUser user = await userManager.FindByIdAsync(Id);
                user.UserName = UserName;
                user.Email = Email;
                IdentityResult result = await userManager.UpdateAsync(user);
                if (result.Succeeded && !string.IsNullOrEmpty(Password))
                {
                    await userManager.RemovePasswordAsync(user);
                    result = await userManager.AddPasswordAsync(user, Password);
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
