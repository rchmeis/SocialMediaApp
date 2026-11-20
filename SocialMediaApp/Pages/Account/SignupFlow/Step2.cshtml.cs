using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models; 
using System.Security.Cryptography;



namespace SocialMediaApp.Pages.Account.SignupFlow
{
    public class Step2 : PageModel
    {

        public string? codeToShow { get; set; } = "";
        public string? userEmail { get; set; } = "";
        public UserAuthentication userAuth { get; set; }
        
        [BindProperty]
        public string? input { get; set; } = "";
        
        public IActionResult OnGet()
        {
            userAuth = HttpContext.Session.GetUserAuthentication();
            if (string.IsNullOrEmpty(userAuth.Email))
            {
                return RedirectToPage("SignupFlow/Step1");
            }

            var verification  = HttpContext.Session.GetVerificationCode();
            
            // Only generate a new code if none exists or the old one expired
            if (verification == null || verification.IsExpired)
            {
                verification = new VerificationCode
                {
                    Code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString(),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                };
                HttpContext.Session.SetVerificationCode(verification);
            }

            codeToShow = verification.Code;

            userEmail = userAuth.Email;
            
            return Page();
        }

        public IActionResult OnPost()
        {
            if (input == codeToShow)
            {
                userAuth.IsVerified = true;
                
                return RedirectToPage("SignupFlow/Step3");
            }
            else
            {
                return Page();
            }
        }
    }
}
