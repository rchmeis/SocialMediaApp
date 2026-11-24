using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp.Pages
{
    public class FeedModel : PageModel
    {
        private readonly IPostService _postService;

        public List<Post> Posts { get; set; } = new List<Post>();

        public FeedModel(IPostService postService)
        {
            _postService = postService;
        }

        public async Task<IActionResult> OnGetAsync(string? tab, int? pageNumber, CancellationToken cancellationToken)
        {
            Enum.TryParse(tab, ignoreCase: true, out FeedTab currentTab);

            // If the tab is not valid, redirect to the recommended tab
            if (!IsValidUrl(tab, pageNumber))
            {
                return RedirectToPage("/Feed", new { tab = FeedTab.Recommended.ToString().ToLower(), pageNumber = 1 });
            }

            PaginationOptions queryOptions = new PaginationOptions
            { 
                PageNumber = pageNumber ?? 1,
                PageSize = 20
            };

            switch (currentTab)
            {
                case FeedTab.Recommended:
                    //TODO: Implement logic for fetching recommended posts
                    // break;
                case FeedTab.News:
                    Posts = await _postService.GetNewsFeedAsync(queryOptions, cancellationToken);
                    break;
                default:
                    return RedirectToPage("/Feed", new { tab = FeedTab.Recommended.ToString().ToLower(), pageNumber = 1 });
            }

            Posts = await _postService.GetNewsFeedAsync(queryOptions, cancellationToken);

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
