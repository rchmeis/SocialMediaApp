using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;
using System.ComponentModel.DataAnnotations;


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
        public List<Interest>? allInterests { get; set; }
        
        [BindProperty]
        [StringLength(255, ErrorMessage = "Din bio må højst være 255 tegn.")]
        public string? InputBio { get; set; }
        
        [BindProperty]
        public List<int> SelectedInterestIds { get; set; } = new();
        
        public IActionResult OnGet()
        {
            userAuth = HttpContext.Session.GetUserAuthentication();
            
            if (string.IsNullOrEmpty(userAuth.Email))
            {
                TempData["Error"] = "Kunne ikke fortsætte med onboarding, prøv igen";
                RedirectToPage("/SignupFlow/step1");
            }
            LoadInterests();
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                userAuth = HttpContext.Session.GetUserAuthentication();
                LoadInterests();
                return Page();
            }

            if (SelectedInterestIds.Count == 0 || SelectedInterestIds == null)
            {
                TempData["Error"] = "Du skal vælge mindst 1 interesse!";
                return Page();
            }

            if (SelectedInterestIds.Count > 10)
            {
                TempData["Error"] = "Du må maks vælge 10 interesser!";
            }
            
            HttpContext.Session.SetStudent(userAuth);
            
            return RedirectToPage("/Account/OnboardingFlow/Step2");
        }

        public void LoadInterests()
        {
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