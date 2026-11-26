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

        public Post PostDetails { get; set; } = new Post();

        public bool IsLiked { get; set; } = false;

        public PostDetailsModel(IPostService postService, ILikeService likeService)
        {
            _postService = postService;
            _likeService = likeService;
        }

        public async Task<IActionResult> OnGetAsync(int postId, CancellationToken cancellationToken)
        {
            Post? post = await _postService.ReadByIdAsync(postId, cancellationToken);

            if (post == null)
            {
                return NotFound();
            }

            PostDetails = post;

            int userId = ClaimsPrincipalExtensions.GetUserId(User);

            IsLiked = await _likeService.IsLikedAsync(new PostLike(userId, postId), cancellationToken);

            return Page();
        }

        public async Task<IActionResult> OnPostLikeAsync(int postId, CancellationToken cancellationToken)
        {
            Post? post = await _postService.ReadByIdAsync(postId, cancellationToken);

            if (post == null)
            {
                return NotFound();
            }

            int userId = ClaimsPrincipalExtensions.GetUserId(User);

            PostLike key = new PostLike() { 
                UserId = userId, 
                PostId = postId 
            };

            await _likeService.LikeAsync(key, cancellationToken);

            return RedirectToPage(new { postId });
        }

        public async Task<IActionResult> OnPostUnlikeAsync(int postId, CancellationToken cancellationToken)
        {
            Post? post = await _postService.ReadByIdAsync(postId, cancellationToken);

            if (post == null)
            {
                return NotFound();
            }

            int userId = ClaimsPrincipalExtensions.GetUserId(User);

            PostLike key = new PostLike()
            {
                UserId = userId,
                PostId = postId
            };

            await _likeService.UnlikeAsync(key, cancellationToken);

            return RedirectToPage(new { postId });
        }
    }
}
