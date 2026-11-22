using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp.Pages.Account.OnboardingFlow
{
    public class Step1Model : PageModel
    {

        private readonly IInterestService _interestService;

        public Step1Model(IInterestService interestService)
        {
            _interestService = interestService;
        }
        
        public UserAuthentication userAuth { get; set; }
        public List<Interest> allInterests { get; set; }
        
        [BindProperty] public string? inputBio { get; set; }
        [BindProperty] public List<String> inputInterests { get; set; }
        
        public void OnGet()
        {
            userAuth = HttpContext.Session.GetUserAuthentication();
            
            if (string.IsNullOrEmpty(userAuth.Email))
            {
                TempData["Error"] = "Kunne ikke fortsætte med onboarding, prøv igen";
                RedirectToPage("/SignupFlow/step1");
            }

            try
            {
                allInterests = _interestService.ReadAll();
            }
            catch
            {
                TempData["Error"] = "Something went wrong getting all interests, try again later";
            }
        }
    }
}