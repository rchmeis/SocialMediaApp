using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp.Pages.Account.OnboardingFlow
{
    public class Step3Model : PageModel
    {
        private readonly IImageService _imageService;
        private readonly ILogger<Step3Model> _logger;

        public Step3Model(IImageService imageService, ILogger<Step3Model> logger)
        {
            _imageService = imageService;
            _logger = logger;
        }

        public Student? student { get; set; }
        public UserAuthentication? userAuth { get; set; }
        public Image? ProfilePicture { get; set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            // If there's no onboarding data in session, user gets sent back to own profile
            if (HttpContext.Session.GetString(SessionKeys.SignupStudent) is null)
            {
                return RedirectToPage("/Account/Profile/Index");
            }

            student = HttpContext.Session.GetStudent();
            userAuth = HttpContext.Session.GetUserAuthentication();
            
            // Step 2 attaches the saved image to the student before storing it in session
            if (student?.Image?.ImageData is { Length: > 0 })
            {
                ProfilePicture = student.Image;
            }

            HttpContext.Session.Remove(SessionKeys.SignupStudent);
            HttpContext.Session.Remove(SessionKeys.SignupAuth);

            return Page();
        }
    }
}