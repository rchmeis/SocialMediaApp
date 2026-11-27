using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Security;
using SocialMediaAppLibrary.Services.Interfaces;
using System.Security.Cryptography;

namespace SocialMediaApp.Pages.Account.SignupFlow
{
    
    /// <summary>
    /// PageModel for the second step of Student account registration. Generates a six-digit verification
    /// code stored in session, validates the code entered by the user (with attempt limit and resend
    /// cooldown), and saves the verified <see cref="UserAuthentication"/> to the database.
    /// </summary>
    
    public class Step2 : PageModel
    {
        private readonly IUserAuthenticationService _userAuthService;
        private readonly IVerificationService _verificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Step2"/> class.
        /// </summary>
        /// <param name="userAuthService">Service used for duplicate checks and saving the user authentication record.</param>
        /// <param name="verificationService">Service used to validate the code, track failed attempts and the resend cooldown.</param>
        public Step2(IUserAuthenticationService userAuthService, IVerificationService verificationService)
        {
            _userAuthService = userAuthService;
            _verificationService = verificationService;
        }

        /// <summary>
        /// Gets or sets the current verification code shown on the page. Temporary: simulates the code
        /// being sent by email and should be removed once real email sending is added.
        /// </summary>
        public string? codeToShow { get; set; }  
        
        /// <summary>
        /// Gets or sets the partially hidden email address shown to the user (for example a***@stud.zealand.dk).
        /// </summary>
        public string? userEmail { get; set; }
        
        /// <summary>
        /// Gets or sets the error message shown to the user, for example a wrong or expired code.
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Gets or sets the number of seconds left before a new code can be requested.
        /// Used for the countdown on the "Send ny kode" button. 0 means a new code can be sent.
        /// </summary>
        public int ResendSeconds { get; set; }

        /// <summary>
        /// Gets or sets the verification code typed in by the user.
        /// </summary>
        [BindProperty]
        public string? input { get; set; }
        
        
        /// <summary>
        /// Handles HTTP GET requests by checking that signup data exists in session, that the email and
        /// username are still available, and generating a verification code if none exists or the old one expired.
        /// </summary>
        /// <returns>
        /// The page with the code form, a redirect to Step 1 if the session is empty or the email/username was
        /// taken in the meantime, or a redirect to Step 3 if the account is already saved.
        /// </returns>
        public async Task<IActionResult> OnGetAsync()
        {
            var userAuth = HttpContext.Session.GetUserAuthentication();

            if (string.IsNullOrEmpty(userAuth.Email))
            {
                TempData["Error"] = "Start forfra med at oprette din bruger.";
                return RedirectToPage("Step1");
            }

            // Already saved (back button)
            if (userAuth.Id != 0)
            {
                return RedirectToPage("Step3");
            }

            
            DuplicateAccountCheck duplicates = await _userAuthService.CheckDuplicateAccountAsync(
                userAuth.Email, userAuth.Username, HttpContext.RequestAborted);

            if (duplicates.EmailExists || duplicates.UsernameExists)
            {
                TempData["Error"] = "Brugernavn eller Email allerede taget, prøv igen";
                return RedirectToPage("Step1");
            }

            var verification = HttpContext.Session.GetVerificationCode();

            // Only generate a new code if none exists or the old one expired
            if (verification == null || verification.IsExpired)
            {
                verification = CreateNewCode();
                _verificationService.StartResendTimer(userAuth.Email);
            }

            codeToShow = verification.Code;
            userEmail = MaskEmail(userAuth.Email);
            ResendSeconds = _verificationService.GetSecondsUntilResend(userAuth.Email);

            return Page();
        }

        /// <summary>
        /// Handles HTTP POST requests by validating the entered code. If the code is correct, sets
        /// <see cref="UserAuthentication.IsVerified"/> to true, saves the account to the database, stores it
        /// in session with its new id and redirects to Step 3.
        /// </summary>
        /// <returns>
        /// A redirect to Step 3 when the code is correct, the current page with an error message when the code
        /// is wrong, expired or has too many failed attempts, or a redirect to Step 1 if the session is empty or
        /// the email/username was taken in the meantime.
        /// </returns>
        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
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
            ResendSeconds = _verificationService.GetSecondsUntilResend(userAuth.Email);

            var verification = HttpContext.Session.GetVerificationCode();
            codeToShow = verification?.Code;   
            
            VerifyCheckResult result = _verificationService.CheckCode(userAuth.Email, input, verification);

            if (!result.Succeeded)
            {
                ErrorMessage = result.ErrorMessage;
                return Page();
            }
            
            // Correct code: verify and save to the database
            userAuth.IsVerified = true;

            try
            {
                userAuth = await _userAuthService.WriteAsync(userAuth, cancellationToken);
            }
            catch (Exception ex) when (ex.InnerException is MySqlException { Number: 1062 })
            {
                TempData["Error"] = "Brugernavn eller Email allerede taget, prøv igen";
                return RedirectToPage("Step1");
            }

            HttpContext.Session.SetUserAuthentication(userAuth);   // keeps the Id for Step 3
            HttpContext.Session.Remove(SessionKeys.VerificationCode);

            return RedirectToPage("Step3");
        }

        /// <summary>
        /// Handles the "Send ny kode" button by replacing the current code with a new one, resetting failed
        /// attempts and starting the 60 second cooldown. A new code can be requested once per 60 seconds.
        /// </summary>
        /// <returns>
        /// A redirect to Step 2 when a new code was created, the current page with a countdown message if the
        /// cooldown is still running, or a redirect to Step 1 if the session is empty.
        /// </returns>
        public IActionResult OnPostResend()
        {
            var userAuth = HttpContext.Session.GetUserAuthentication();

            if (string.IsNullOrEmpty(userAuth.Email))
            {
                TempData["Error"] = "Start forfra med at oprette din bruger.";
                return RedirectToPage("Step1");
            }

            int wait = _verificationService.GetSecondsUntilResend(userAuth.Email);

            if (wait > 0)
            {
                userEmail = MaskEmail(userAuth.Email);
                ResendSeconds = wait;
                ErrorMessage = "Vent " + wait + " sekunder, før du kan sende en ny kode.";
                return Page();   
            }

            CreateNewCode();
            _verificationService.ResetAttempts(userAuth.Email);
            _verificationService.StartResendTimer(userAuth.Email);

            return RedirectToPage("Step2");
        }

        /// <summary>
        /// Creates a random six-digit verification code that expires after 15 minutes and saves it in session.
        /// The new code replaces the old one, so the old code is no longer valid.
        /// </summary>
        /// <returns>The newly created <see cref="VerificationCode"/>.</returns>
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

        /// <summary>
        /// This helper method, hides most of the name part of an email address, for example anna@stud.zealand.dk becomes a***@stud.zealand.dk.
        /// </summary>
        /// <param name="email">The email address to mask.</param>
        /// <returns>The masked email address, or the original value if it is not a valid email format.</returns>
        private static string MaskEmail(string email)
        {
            string[] parts = email.Split('@');

            if (parts.Length != 2 || parts[0].Length == 0)
            {
                return email;
            }

            return parts[0][0] + "***@" + parts[1];
        }
    }
}