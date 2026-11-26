using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaApp.Security;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;
using SocialMediaAppLibrary.Validation;

namespace SocialMediaApp.Pages.Posts
{
    public class CreatePostModel : PageModel
    {
        private readonly IPostService _postService;

        public Random RandomGenerator { get; set; } = new Random();
        public string RandomizedPlaceholder { get; set; } = string.Empty;

        // [ÆNDRING] Gemmer hvor brugeren kom fra (fx "/Account/Profile/Index" eller "/Feed"), så den overlever POST og validation-fejl
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

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

        public CreatePostModel(IPostService postService)
        {
            _postService = postService;
        }

        public void OnGet()
        {
            RandomizedPlaceholder = GenerateRandomPlaceholder();
            if (string.IsNullOrWhiteSpace(ReturnUrl) || !Url.IsLocalUrl(ReturnUrl))
            {
                ReturnUrl = "/Feed";
            }
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                // [ÆNDRING] Sørg for at placeholder også er sat hvis siden vises igen ved validation-fejl
                if (string.IsNullOrWhiteSpace(RandomizedPlaceholder))
                {
                    RandomizedPlaceholder = GenerateRandomPlaceholder();
                }
                return Page();
            }

            int userId = ClaimsPrincipalExtensions.GetUserId(User);

            UserSummary author = new UserSummary
            {
                Id = userId
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

                // [ÆNDRING] Redirect til ReturnUrl (f.eks. egen profilside) hvis det er en gyldig lokal URL, ellers tilbage til feedet
                if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl);
                }
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
