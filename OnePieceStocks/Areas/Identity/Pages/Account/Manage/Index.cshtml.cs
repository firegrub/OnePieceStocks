using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnePieceStocks.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public string Username { get; set; } = "";

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Username = user?.UserName ?? "";
        }
    }
}