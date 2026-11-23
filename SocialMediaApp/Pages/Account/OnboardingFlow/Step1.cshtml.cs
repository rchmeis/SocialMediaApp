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
        
        public Student student { get; set; }
        public List<Interest>? allInterests { get; set; }
        
        [BindProperty]
        [StringLength(255, ErrorMessage = "Din bio må højst være 255 tegn.")]
        public string? InputBio { get; set; }
        
        [BindProperty]
        public List<int> SelectedInterestIds { get; set; } = new();
        
        public IActionResult OnGet()
        {
            student = HttpContext.Session.GetStudent();
            
            if (student.IsOnboarded)
            {
                TempData["Error"] = "Du har allerede været igennem OnBoarding!";
                return RedirectToPage("/Feed");
            }
            
            if (string.IsNullOrEmpty(student.FirstName))
            {
                TempData["Error"] = "Kunne ikke fortsætte med onboarding, prøv igen";
                RedirectToPage("/Account/SignupFlow/Step1.");
            }
            
            LoadInterests();

            // Pre-fill if the user comes back to this step
            InputBio = student.Bio;
            SelectedInterestIds = allInterests
                .Where(i => student.Interests.Contains(i.Name))
                .Select(i => i.Id)
                .ToList();
            
            return Page();
        }

        public IActionResult OnPost()
        {
            student = HttpContext.Session.GetStudent();
            LoadInterests();  
            
            if (!ModelState.IsValid)
            {
                student = HttpContext.Session.GetStudent();
                LoadInterests();
                return Page();
            }

            if (SelectedInterestIds == null || SelectedInterestIds.Count == 0)
            {
                TempData["Error"] = "Du skal vælge mindst 1 interesse!";
                LoadInterests();  
                return Page();
            }

            if (SelectedInterestIds.Count > 10)
            {
                TempData["Error"] = "Du må maks vælge 10 interesser!";
                LoadInterests();  
                return Page();  
            }
            
            student.Bio = InputBio;
            student.Interests = allInterests
                .Where(i => SelectedInterestIds.Contains(i.Id))
                .Select(i => i.Name)
                .ToList();
            
            HttpContext.Session.SetStudent(student);
            
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