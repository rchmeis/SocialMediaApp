using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;
using SocialMediaAppLibrary.Validation;

namespace SocialMediaApp.Pages.Posts
{
    public class CreatePostModel : PageModel
    {
        private readonly IPostService _postService;

        private readonly IUserAuthenticationService _userAuthenticationService;

        private readonly IUserService _userService;

        public Random RandomGenerator { get; set; } = new Random();
        public string RandomizedPlaceholder { get; set; } = string.Empty;

        [BindProperty]
        [MaxLength(63, ErrorMessage = "Titlen må højst være 63 tegn.")]
        public string? PostTitle { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Indholdet må ikke være tomt.")]
        [MaxLength(2000, ErrorMessage = "Indholdet må ikke overstige 2000 tegn.")]
        public string? PostContent { get; set; }

        [BindProperty]
        [ListCount(0, 4, ErrorMessage = "Du kan højst vedhæfte 4 billeder pr. opslag.")]
        [AllowedContentTypes(["image/jpg", "image/jpeg", "image/png", "image/webp"], ErrorMessage = "Alle billeder skal være i JPG, PNG eller WebP format.")]
        [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Alle billeder må højst være 5 MB.")]
        public List<IFormFile> PostImages { get; set; } = new List<IFormFile>();

        public CreatePostModel(IPostService postService, IUserAuthenticationService userAuthenticationService, IUserService userService)
        {
            _postService = postService;
            _userAuthenticationService = userAuthenticationService;
            _userService = userService;
        }

        public void OnGet()
        {
            RandomizedPlaceholder = GenerateRandomPlaceholder();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            UserSummary author = new UserSummary
            {
                Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
            };

            Post postToCreate = new Post
            {
                Title = PostTitle,
                Author = author,
                Content = PostContent!
            };

            try
            {
                await _postService.CreateAsync(postToCreate, PostImages, cancellationToken);
                return RedirectToPage("/Feed");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Dit opslag kunne ikke gemmes. Prøv igen.");
                return Page();
            }
        }

        private string GenerateRandomPlaceholder()
        {
            List<string> placeholders = new List<string>()
            {
                "Hvad har du på hjerte?",
                "Hvad er dit næste projekt?",
                "Hvad inspirerer dig i dag?",
                "Fortæl os om din dag!",
                "Hvilke oplevelser vil du dele?",
            };

            return placeholders[RandomGenerator.Next(placeholders.Count)];
        }
    }
}
