using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnlineSchool2.Pages
{
    [Authorize(Roles = "Coach")]
    public class CoachPageModel : PageModel
    {
    }
}
