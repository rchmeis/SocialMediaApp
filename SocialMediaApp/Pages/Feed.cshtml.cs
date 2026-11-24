using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp.Pages
{
    public class FeedModel : PageModel
    {
        private readonly IPostService _postService;

        public FeedModel(IPostService postService)
        {
            _postService = postService;
        }

        public async Task<IActionResult> OnGetAsync(string? tab, int? pageNumber)
        {
            if (!IsValidUrl(tab, pageNumber))
            {
                return RedirectToPage("/Feed", new { tab = FeedTab.Recommended.ToString().ToLower(), pageNumber = 1 });
            }

            return Page();
        }

        private bool IsValidUrl(string? tab, int? pageNumber)
        {
            if (string.IsNullOrEmpty(tab) || !Enum.TryParse(tab, ignoreCase: true, out FeedTab _))
            {
                return false;
            }

            if (pageNumber == null || pageNumber < 1)
            {
                return false;
            }

            return true;
        }
    }
}
