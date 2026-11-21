using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models; 
using SocialMediaAppLibrary.Services.Interfaces;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;

namespace SocialMediaApp.Pages.Account.SignupFlow
{
    public class Step2 : PageModel
    {

        private readonly IUserAuthenticationService _userAuthService;

        public Step2(IUserAuthenticationService userAuthService)
        {
            _userAuthService = userAuthService;
        }
        
        public string? codeToShow { get; set; } = "";
        public string? userEmail { get; set; } = "";
        public UserAuthentication userAuth { get; set; }
        public string? ErrorMessage { get; set; }
        public int codeCount { get; set; } = 0;
        
        [BindProperty]
        public string? input { get; set; } = "";
        
        public async Task<IActionResult> OnGetAsync()
        {
            userAuth = HttpContext.Session.GetUserAuthentication();
            if (string.IsNullOrEmpty(userAuth.Email))
            {
                TempData["Error"] = "Start forfra med at oprette din bruger.";
                return RedirectToPage("Step1");
            }
            
            if (userAuth.Id != 0)
            {
                return RedirectToPage("Step3");
            }
            
            // Checks if email already exists
            DuplicateAccountCheck duplicateAccountCheck = await _userAuthService.CheckDuplicateAccountAsync(userAuth.Email, userAuth.Username, HttpContext.RequestAborted);

            if (duplicateAccountCheck.EmailExists || duplicateAccountCheck.UsernameExists)
            {
                TempData["Error"] = "Brugernavn eller Email allerede taget, prøv igen";
                return RedirectToPage("Step1");
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

            userEmail = MaskEmail(userAuth.Email);
            
            return Page();
        }

        public IActionResult OnPost()
        { 
            var userAuth = HttpContext.Session.GetUserAuthentication();
            
            if (string.IsNullOrEmpty(userAuth.Email))
                {
                    TempData["Error"] = "Start forfra med at oprette din bruger.";
                    return RedirectToPage("Step1");
                }

                // Already saved (back button / double submit)
                if (userAuth.Id != 0)
                {
                    return RedirectToPage("Step3");
                }

                userEmail = MaskEmail(userAuth.Email);

                var verification = HttpContext.Session.GetVerificationCode();
                
                codeToShow = verification.Code;

                if (verification == null || verification.IsExpired)
                {
                    ErrorMessage = "Verifikations kode er udløbet! Tryk på ´Send ny kode´";
                    return Page();
                }

                if (input == null || !System.Text.RegularExpressions.Regex.IsMatch(input, @"^\d{6}$"))
                {
                    ErrorMessage = "Koden skal være præcis 6 cifre.";
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(input) || input.Trim() != verification.Code)
                {
                    ErrorMessage = "Forkert kode.";
                    codeCount = codeCount + 1;
                    return Page();
                }

                userAuth.IsVerified = true;
                
                try
                {
                    userAuth = _userAuthService.Write(userAuth);   
                }
                catch (Exception ex) when (ex.InnerException is MySqlException { Number: 1062 })
                {
                    TempData["Error"] = "Brugernavn eller Email allerede taget, prøv igen";
                    return RedirectToPage("Step1");
                }
                
                HttpContext.Session.SetUserAuthentication(userAuth); // Sets the Id for Step 3
                HttpContext.Session.Remove(SessionKeys.VerificationCode);

                return RedirectToPage("Step3");
        }
        
        public IActionResult OnPostResend()
        {
            var userAuth = HttpContext.Session.GetUserAuthentication();
            if (string.IsNullOrEmpty(userAuth.Email))
            {
                return RedirectToPage("Step1");
            }

            CreateNewCode();                 
            return RedirectToPage("Step2");  
        }
        
        private VerificationCode CreateNewCode()
        {
            var verification = new VerificationCode
            {
                Code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
            HttpContext.Session.SetVerificationCode(verification);
            return verification;
        }
        
        // Helper method to mask email.
        private static string MaskEmail(string email)
        {
            string[] parts = email.Split('@'); // splits the string in two from "@"

            // Makes sure, it doesn't crash if email got more or no "@"
            if (parts.Length != 2 || parts[0].Length == 0)
            {
                return email;    
            }
            
            string name = parts[0];
            string domain = parts[1];

            return name[0] + "***" + "@" + domain;
        }
    }
}
