using Microsoft.AspNetCore.Identity;
using NuGet.Protocol.Core.Types;
using System.Data;

namespace OnlineSchool2.Models
{
    public class EFIdentityRepository : IIdentityRepository
    {
        private UserManager<IdentityUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        private SignInManager<IdentityUser> signInManager;

        public IQueryable<IdentityUser> Users => userManager.Users;
        public IQueryable<IdentityRole> Roles => roleManager.Roles;

        public EFIdentityRepository(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
        }

        public async Task<IdentityResult> CreateUser(IdentityUser u, string p)
        {
            return await userManager.CreateAsync(u, p);
        }
        public async Task<IdentityResult> AddUserToRole(IdentityUser u, IdentityRole r)
        {
            return await userManager.AddToRoleAsync(u, r.Name);
        }
        public async Task<IdentityResult> DeleteUser(IdentityUser u)
        {
            return await userManager.DeleteAsync(u);
        }
        
        public void CreateRole(IdentityRole r)
        {

        }
        public void DeleteRole(IdentityRole r)
        {

        }
    }
}
