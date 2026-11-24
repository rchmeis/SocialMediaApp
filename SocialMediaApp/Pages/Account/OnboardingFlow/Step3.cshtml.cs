using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Pqc.Crypto.SphincsPlus;
using SocialMediaAppLibrary.Models;

namespace SocialMediaApp.Pages.Account.OnboardingFlow
{
    public class Step3Model : PageModel
    {
        
        public Student student { get; set; }
        public UserAuthentication userAuth { get; set; }
        
        public IActionResult OnGet()
        {
            // If there's no onboarding data in session, user gets sen back to own profile
            if (HttpContext.Session.GetString(SessionKeys.SignupStudent) is null)
            {
                return RedirectToPage("/Account/Profile/Index");
            }
            
            student = HttpContext.Session.GetStudent();
            userAuth = HttpContext.Session.GetUserAuthentication();

            HttpContext.Session.Remove(SessionKeys.SignupStudent);
            HttpContext.Session.Remove(SessionKeys.SignupAuth);

            return Page();
        }
    }
}
