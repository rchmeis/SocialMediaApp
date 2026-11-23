using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SocialMediaApp.Pages.Account.OnboardingFlow
{
    public class Step1Model : PageModel
    {

        private readonly IInterestService _interestService;
        private readonly IStudentService _studentService;

        public Step1Model(IInterestService interestService, IStudentService studentService)
        {
            _interestService = interestService;
            _studentService = studentService;
        }
        
        public Student student { get; set; }
        public List<Interest>? allInterests { get; set; } = new(); 
        
        [BindProperty]
        [StringLength(255, ErrorMessage = "Din bio må højst være 255 tegn.")]
        public string? InputBio { get; set; }
        
        [BindProperty]
        public List<int> SelectedInterestIds { get; set; } = new();
        
        public IActionResult OnGet()
        {
            if (User.FindFirstValue("IsOnboarded") == "true")
                return RedirectToPage("/Feed");

            student = GetOnboardingStudent();
            
            if (student == null)
            {
                TempData["Error"] = "Kunne ikke fortsætte med onboarding, prøv igen";
                return RedirectToPage("/Account/SignupFlow/Step3");
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
            if (User.FindFirstValue("IsOnboarded") == "true")
                return RedirectToPage("/Feed");

            student = GetOnboardingStudent();
            LoadInterests();  
            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (SelectedInterestIds == null || SelectedInterestIds.Count == 0)
            {
                TempData["Error"] = "Du skal vælge mindst 1 interesse!";
                return Page();
            }

            if (SelectedInterestIds.Count > 10)
            {
                TempData["Error"] = "Du må maks vælge 10 interesser!";
                return Page();  
            }
            
            student.Bio = InputBio?.Trim() ?? string.Empty;
            student.Interests = allInterests
                .Where(i => SelectedInterestIds.Contains(i.Id))
                .Select(i => i.Name)
                .ToList();
            
            HttpContext.Session.SetStudent(student);
            
            return RedirectToPage("/Account/OnboardingFlow/Step2");
        }
        
        // Session holds the onboarding answers; the id comes from the login cookie.
        private Student GetOnboardingStudent()
        {
            Student student = HttpContext.Session.GetStudent();

            if (student.Id == 0 && int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id))
            {
                student.Id = id;
            }

            return student;
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