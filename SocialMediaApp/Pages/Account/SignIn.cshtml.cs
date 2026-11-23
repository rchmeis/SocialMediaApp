using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Security;
using SocialMediaAppLibrary.Services.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SocialMediaApp.Pages.Account
{
    public class SignInModel : PageModel
    {
        private readonly IUserAuthenticationService _userAuthenticationService;
        private readonly IUserService _userService;

        public SignInModel(IUserAuthenticationService userAuthenticationservice, IUserService userService)
        {
            _userAuthenticationService = userAuthenticationservice;
            _userService = userService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        // Modtag returnUrl som en parameter fra URL'en via [BindProperty(SupportsGet = true)]
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }


        public class InputModel
        {
            [Required]
            public string EmailOrUsername { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }


        public void OnGet()
        {
        }

        /// <summary>
        /// Handles the HTTP POST request for user login. 
        /// Authenticates credentials via <see cref="IUserAuthenticationService.AuthenticateAsync"/>, 
        /// adds any returned error message to <see cref="PageModel.ModelState"/> on failure, redirects 
        /// users who have not yet completed profile registration to <c>/Account/SignupFlow/Step3</c>, 
        /// or otherwise signs the user in with persistent or session-bound cookie authentication and 
        /// redirects to <c>ReturnUrl</c> (if local) or a role-specific landing page.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an 
        /// <see cref="IActionResult"/> redirect to registration, <c>ReturnUrl</c>, or the home/profile 
        /// page, or re-renders the page with model state errors if validation or authentication fails.
        /// </returns>
        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return Page();

            LoginCheckResult result = await _userAuthenticationService.AuthenticateAsync(Input.EmailOrUsername, Input.Password, cancellationToken);
            
            if (!result.Succeeded || result.User is null)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Ugyldigt login.");
                return Page();
            }

            UserSummary userSummary = await _userService.GetByUserAuthIdAsync(result.User.Id, cancellationToken);
            if (userSummary is null)
            {
                
                List<Claim> claimsNotOnboarded = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
                    new Claim(ClaimTypes.Role, result.User.Role.ToString()),
                    new Claim(ClaimTypes.Name, result.User.Username??result.User.Email),
                    new Claim("IsOnboarded", "false")
                };
                ClaimsIdentity claimsIdentityNotOnboarded = new ClaimsIdentity(claimsNotOnboarded, CookieAuthenticationDefaults.AuthenticationScheme);

                AuthenticationProperties authPropertiesNotOnboarded = new AuthenticationProperties
                {
                    // IsPersistent = true means that the cookie is saved in the harddisk and survives even if the browser is closed.
                    // IsPersistent = false turns it into a "session cookie", that is deleted, when the browser is closed.
                    IsPersistent = Input.RememberMe,

                    // If "Remember me" is ticked the cookie will expire in 14 days. Otherwise it is set to null and will only survive the session.
                    ExpiresUtc = Input.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : null
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentityNotOnboarded),
                    authPropertiesNotOnboarded);  //NOTICE! authProperties added!

                return RedirectToPage("/Account/SignupFlow/Step3");  //if the user verified their account but did not complete registration
            }


            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
                new Claim(ClaimTypes.Role, result.User.Role.ToString()),
                new Claim(ClaimTypes.Name, userSummary.DisplayName),
                new Claim("ImageId", userSummary.image.Id.ToString()),
                new Claim("IsOnboarded", userSummary.isOnboarded ? "true" : "false")   

            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            AuthenticationProperties authProperties = new AuthenticationProperties
            {
                // IsPersistent = true means that the cookie is saved in the harddisk and survives even if the browser is closed.
                // IsPersistent = false turns it into a "session cookie", that is deleted, when the browser is closed.
                IsPersistent = Input.RememberMe,

                // If "Remember me" is ticked the cookie will expire in 14 days. Otherwise it is set to null and will only survive the session.
                ExpiresUtc = Input.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : null
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);  //NOTICE! authProperties added!


            // Security check: Url.IsLocalUrl(ReturnUrl) prevents Open Redirect-attacks (ensures that it is local)
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl); // Send the user back where they came from!
            }

            if (result.User.Role is Role.Student) return RedirectToPage("/Feed");
            return RedirectToPage("/Account/Profile/Index");

        }
    }
}
