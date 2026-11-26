using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SocialMediaApp.Security;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp.Pages
{
    public class FeedModel : PageModel
    {
        private readonly IPostService _postService;
        private readonly ILikeService _likeService;

        public GenericPaginatedResult<Post> Posts { get; set; } = new GenericPaginatedResult<Post>();

        public HashSet<int> LikedPostIds { get; set; } = new HashSet<int>();

        [BindProperty(SupportsGet = true)]
        [FromQuery(Name = "page")]
        public int PageNumber { get; set; } = 1;

        public Dictionary<FeedTab, string> FeedTabs { get; set; } = new Dictionary<FeedTab, string>
        {
            { FeedTab.Recommended, "For dig" },
            { FeedTab.Newest, "Nyeste" }
        };

        [BindProperty(SupportsGet = true)]
        [FromQuery(Name = "tab")]
        public string CurrentTab { get; set; } = string.Empty;

        public FeedModel(IPostService postService, ILikeService likeService)
        {
            _postService = postService;
            _likeService = likeService;
        }

        public async Task<IActionResult> OnGetAsync(string? tab, CancellationToken cancellationToken)
        {
            Enum.TryParse(tab, ignoreCase: true, out FeedTab currentTab);
            CurrentTab = currentTab.ToString().ToLower();

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
                    Posts = await _postService.GetNewestFeedAsync(queryOptions, cancellationToken);
                    break;
                default:
                    return RedirectToPage("/Feed", new { tab = FeedTab.Recommended.ToString().ToLower(), page = 1 });
            }

            int userId = ClaimsPrincipalExtensions.GetUserId(User);

            // To determine which posts are liked by the user, we need to check each post in the current page and assign the liked post IDs to the LikedPostIds HashSet
            foreach (Post post in Posts.Items)
            {
                bool isLiked = await _likeService.IsLikedAsync(new PostLike(userId, post.Id), cancellationToken);

                if (isLiked)
                {
                    LikedPostIds.Add(post.Id);
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostLikeAsync(int postId, string tab, int pageNumber, CancellationToken cancellationToken)
        {
            Post? post = await _postService.ReadByIdAsync(postId, cancellationToken);

            if (post == null)
            {
                return NotFound();
            }

            int userId = ClaimsPrincipalExtensions.GetUserId(User);

            PostLike key = new PostLike(userId, postId);

            await _likeService.LikeAsync(key, cancellationToken);

            return Redirect($"{Url.Page("/Feed", new { tab, page = pageNumber })}#post-{postId}");
        }

        public async Task<IActionResult> OnPostUnlikeAsync(int postId, string tab, int pageNumber, CancellationToken cancellationToken)
        {
            Post? post = await _postService.ReadByIdAsync(postId, cancellationToken);

            if (post == null)
            {
                return NotFound();
            }

            int userId = ClaimsPrincipalExtensions.GetUserId(User);

            PostLike key = new PostLike(userId, postId);

            await _likeService.UnlikeAsync(key, cancellationToken);

            return Redirect($"{Url.Page("/Feed", new { tab, page = pageNumber })}#post-{postId}");
        }

        /// <summary>
        /// Generates the URL for a specific page in the feed, preserving the current tab.
        /// </summary>
        /// <param name="page">The page number to generate the URL for.</param>
        /// <returns>The URL for the specified page.</returns>
        public string GetPageUrl(int page)
        {
            string url = Url.Page("/Feed", new { tab = CurrentTab.ToString().ToLower() })!;

            return QueryHelpers.AddQueryString(url, "page", page.ToString());
        }

        /// <summary>
        /// Validates the tab and page number parameters from the URL.
        /// </summary>
        /// <param name="tab">The tab parameter from the URL.</param>
        /// <param name="pageNumber">The page number parameter from the URL.</param>
        /// <returns>True if the parameters are valid; otherwise, false.</returns>
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
