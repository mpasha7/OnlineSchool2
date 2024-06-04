using Microsoft.AspNetCore.Identity;

namespace OnlineSchool2.Models
{
    public interface IIdentityRepository
    {
        IQueryable<IdentityUser> Users { get; }
        IQueryable<IdentityRole> Roles { get; }

        Task<IdentityResult> CreateUser(IdentityUser u, string p);
        Task<IdentityResult> AddUserToRole(IdentityUser u, IdentityRole r);
        Task<IdentityResult> DeleteUser(IdentityUser u);

        void CreateRole(IdentityRole r);
        void DeleteRole(IdentityRole r);
    }
}
