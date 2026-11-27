using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaApp.Security;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp.Pages.Posts
{
    public class PostDetailsModel : PageModel
    {
        private readonly IPostService _postService;
        private readonly ILikeService _likeService;

        /// <summary>
        /// Gets or sets the post being displayed.
        /// </summary>
        public Post PostDetails { get; set; } = new Post();

        /// <summary>
        /// Gets or sets a value indicating whether the current user has liked the post.
        /// </summary>
        public bool IsLiked { get; set; } = false;

        /// <summary>
        /// Gets or sets the user ID of the currently signed-in user.
        /// </summary>
        public int LoggedInUserId { get; set; }

        /// <summary>
        /// Gets or sets the raw return URL bound from the query string or form.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        /// <summary>
        /// Gets or sets a validated local return URL, defaulting to the Feed page.
        /// </summary>
        public string SafeReturnUrl { get; set; } = "/Feed";


        /// <summary>
        /// Initializes a new instance of the <see cref="PostDetailsModel"/> class.
        /// </summary>
        /// <param name="postService">Service used to retrieve post details.</param>
        /// <param name="likeService">Service used to check and toggle post likes.</param>
        public PostDetailsModel(IPostService postService, ILikeService likeService)
        {
            _postService = postService;
            _likeService = likeService;
        }

        /// <summary>
        /// Handles GET requests for a post's details page.
        /// </summary>
        /// <param name="postId">The identifier of the post to load.</param>
        /// <param name="returnUrl">Optional local return URL from the referring page.</param>
        /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
        /// <returns>The rendered page or <see cref="NotFoundResult"/> if the post does not exist.</returns>
        public async Task<IActionResult> OnGetAsync(string? returnUrl, int postId, CancellationToken cancellationToken)
        {
            Post? post = await _postService.ReadByIdAsync(postId, cancellationToken);

            if (post == null)
            {
                return NotFound();
            }

            PostDetails = post;
            LoggedInUserId = ClaimsPrincipalExtensions.GetUserId(User);
            SafeReturnUrl = ResolveSafeReturnUrl(returnUrl);


            int userId = ClaimsPrincipalExtensions.GetUserId(User);

            IsLiked = await _likeService.IsLikedAsync(new PostLike(userId, postId), cancellationToken);

            return Page();
        }


        /// <summary>
        /// Handles POST requests to like the current post and redirect back to the details page.
        /// </summary>
        /// <param name="postId">The identifier of the post to like.</param>
        /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
        /// <returns>A redirect to the post details page or <see cref="NotFoundResult"/>.</returns>
        public async Task<IActionResult> OnPostLikeAsync(int postId, CancellationToken cancellationToken)
        {
            Post? post = await _postService.ReadByIdAsync(postId, cancellationToken);

            if (post == null)
            {
                return NotFound();
            }

            LoggedInUserId = ClaimsPrincipalExtensions.GetUserId(User);
            SafeReturnUrl = ResolveSafeReturnUrl(ReturnUrl);            

            PostLike key = new PostLike() { 
                UserId = LoggedInUserId, 
                PostId = postId 
            };

            await _likeService.LikeAsync(key, cancellationToken);

            return RedirectToPage(new { postId, returnUrl = ReturnUrl });
        }

        /// <summary>
        /// Handles POST requests to remove a like from the current post and redirect back to the details page.
        /// </summary>
        /// <param name="postId">The identifier of the post to unlike.</param>
        /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
        /// <returns>A redirect to the post details page or <see cref="NotFoundResult"/>.</returns>
        public async Task<IActionResult> OnPostUnlikeAsync(int postId, CancellationToken cancellationToken)
        {
            Post? post = await _postService.ReadByIdAsync(postId, cancellationToken);

            if (post == null)
            {
                return NotFound();
            }

            LoggedInUserId = ClaimsPrincipalExtensions.GetUserId(User);
            SafeReturnUrl = ResolveSafeReturnUrl(ReturnUrl);

            PostLike key = new PostLike()
            {
                UserId = LoggedInUserId,
                PostId = postId
            };

            await _likeService.UnlikeAsync(key, cancellationToken);

            return RedirectToPage(new { postId, returnUrl = ReturnUrl });
        }

        /// <summary>
        /// Validates that <paramref name="candidateUrl"/> is a local URL and falls back to <c>/Feed</c>.
        /// </summary>
        /// <param name="candidateUrl">The candidate URL to validate.</param>
        /// <returns>A safe local URL string.</returns>
        private string ResolveSafeReturnUrl(string? candidateUrl)
        {
            if (!string.IsNullOrWhiteSpace(candidateUrl) && Url.IsLocalUrl(candidateUrl))
            {
                return candidateUrl;
            }

            return Url.Page("/Feed") ?? "/Feed";
        }
    }
}
