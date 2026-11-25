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

        [BindProperty(SupportsGet = true)]
        [FromQuery(Name = "page")]
        public int PageNumber { get; set; } = 1;

        public FeedModel(IPostService postService)
        {
            _postService = postService;
        }

        public async Task<IActionResult> OnGetAsync(string? tab, CancellationToken cancellationToken)
        {
            Enum.TryParse(tab, ignoreCase: true, out FeedTab currentTab);

            // If the tab is not valid, redirect to the recommended tab
            if (!IsValidUrl(tab, PageNumber))
            {
                return RedirectToPage("/Feed", new { tab = FeedTab.Recommended.ToString().ToLower(), page = 1 });
            }

            PaginationOptions queryOptions = new PaginationOptions
            { 
                PageNumber = PageNumber,
                PageSize = 20
            };

            switch (currentTab)
            {
                case FeedTab.Recommended:
                    //TODO: Implement logic for fetching recommended posts
                    // break;
                case FeedTab.Newest:
                    Posts = await _postService.GetNewsFeedAsync(queryOptions, cancellationToken);
                    break;
                default:
                    return RedirectToPage("/Feed", new { tab = FeedTab.Recommended.ToString().ToLower(), page = 1 });
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
