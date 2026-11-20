using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp.Pages.Posts
{
    public class CreatePostModel : PageModel
    {
        private readonly IPostService _postService;

        public Random RandomGenerator { get; set; } = new Random();
        public string RandomizedPlaceholder { get; set; } = string.Empty;

        [BindProperty]
        public string? PostTitle { get; set; }

        [BindProperty]
        public string? PostContent { get; set; }

        [BindProperty]
        public List<IFormFile> PostImages { get; set; } = new List<IFormFile>();

        public CreatePostModel(IPostService postService)
        {
            _postService = postService;
        }

        public void OnGet()
        {
            RandomizedPlaceholder = GenerateRandomPlaceholder();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ValidatePostTitleAndContent())
            {
                return Page();
            }

            if (!ValidatePostImages())
            {
                return Page();
            }

            Post postToCreate = new Post
            {
                Title = PostTitle,
                Content = PostContent!
            };

            // await _postService.CreateAsync(postToCreate, PostImages);

            return RedirectToPage("/Feed");
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

        private bool ValidatePostTitleAndContent()
        {
            if (PostTitle != null && PostTitle.Length > 63)
            {
                ModelState.AddModelError(nameof(PostTitle), "Titlen må højst være 63 tegn.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PostContent))
            {
                ModelState.AddModelError(nameof(PostContent), "Indholdet må ikke være tomt.");
                return false;
            }

            if (PostContent.Length > 2000)
            {
                ModelState.AddModelError(nameof(PostContent), "Indholdet må ikke overstige 2000 tegn.");
                return false;
            }

            return true;
        }

        private bool ValidatePostImages()
        {
            string[] acceptedContentTypes = [ "image/jpg", "image/jpeg", "image/png", "image/webp" ];

            if (PostImages.Count > 4)
            {
                ModelState.AddModelError(nameof(PostImages), "Du kan højst vedhæfte 4 billeder pr. opslag.");
                return false;
            }

            foreach (IFormFile image in PostImages)
            {
                if (!acceptedContentTypes.Contains(image.ContentType))
                {
                    ModelState.AddModelError(nameof(PostImages), "Alle billeder skal være i JPG, PNG eller WebP format.");
                    return false;
                }

                if (image.Length > 5 * 1024 * 1024) // Max 5 MB image size
                {
                    ModelState.AddModelError(nameof(PostImages), "Alle billeder må højst være 5 MB.");
                    return false;
                }
            }

            return true;
        }
    }
}
