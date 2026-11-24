using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp.Pages.Posts
{
    public class PostDetailsModel : PageModel
    {
        private readonly IPostService _postService;

        public Post PostDetails { get; set; } = new Post();

        public PostDetailsModel(IPostService postService)
        {
            _postService = postService;
        }

        public async Task<IActionResult> OnGetAsync(int postId)
        {
            Post? post = await _postService.ReadByIdAsync(postId);

            if (post == null)
            {
                return NotFound();
            }

            PostDetails = post;

            return Page();
        }
    }
}
