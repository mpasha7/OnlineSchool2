using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineSchool2.Models;

namespace OnlineSchool2.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<IdentityUser> userManager;
        private SignInManager<IdentityUser> signInManager;

        public AccountController(UserManager<IdentityUser> userMgr, SignInManager<IdentityUser> signInMgr)
        {
            this.userManager = userMgr;
            this.signInManager = signInMgr;
        }

        public IActionResult Login(string returnUrl)
        {
            return View(new LoginModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                IdentityUser? user = await userManager.FindByNameAsync(loginModel.Name);
                if (user is not null)
                {
                    await signInManager.SignOutAsync();
                    if ((await signInManager.PasswordSignInAsync(user, loginModel.Password, false, false)).Succeeded)
                    {
                        if (await userManager.IsInRoleAsync(user, "Admin"))
                        {
                            return Redirect("/users/list");
                        }
                        return Redirect(loginModel?.ReturnUrl ?? "/courses");
                    }
                }
            }
            ModelState.AddModelError("", "Неверный логин или пароль");
            return View(loginModel);
        }

        [Authorize]
        public async Task<RedirectResult> Logout(string returnUrl = "/")
        {
            await signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            IdentityUser? user = await userManager.FindByNameAsync(User.Identity.Name);
            //if (await userManager.IsInRoleAsync(user, "Admin"))
            //{

            //}
            return View(user);
        }

        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            IdentityUser? user = await userManager.FindByNameAsync(User.Identity.Name);
            return View(new UserModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EditProfile(UserModel model)
        {
            
            if (ModelState.IsValid)
            {
                IdentityUser user = await userManager.FindByIdAsync(model.Id);
                if (await userManager.CheckPasswordAsync(user, model.OldPassword))
                {
                    user.UserName = model.UserName;
                    user.Email = model.Email;
                    IdentityResult result = await userManager.UpdateAsync(user);
                    var signInResult = new Microsoft.AspNetCore.Identity.SignInResult();
                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrWhiteSpace(model.NewPassword))
                        {
                            await userManager.RemovePasswordAsync(user);
                            result = await userManager.AddPasswordAsync(user, model.NewPassword);
                            await signInManager.SignOutAsync();
                            signInResult = await signInManager.PasswordSignInAsync(user, model.NewPassword, false, false);
                        }
                        else
                        {
                            await signInManager.SignOutAsync();
                            signInResult = await signInManager.PasswordSignInAsync(user, model.OldPassword, false, false);
                        }
                    }
                    if (result.Succeeded && signInResult.Succeeded)
                    {
                        return RedirectToAction(nameof(Profile));
                    }
                    foreach (IdentityError err in result.Errors)
                    {
                        ModelState.AddModelError("", err.Description);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Пароль не верен");
                }
            }
            return View(model);
        }
    }
}
