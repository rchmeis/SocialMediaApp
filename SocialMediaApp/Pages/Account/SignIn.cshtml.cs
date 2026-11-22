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

    /// <summary>
    /// Razor PageModel responsible for authenticating existing users and routing them
    /// either to profile registration (<c>/Account/SignupFlow/Step3</c>) or to their post-login destination.
    /// </summary>
    public class SignInModel : PageModel
    {
        private readonly IUserAuthenticationService _userAuthenticationService;
        private readonly IUserService _userService;


        /// <summary>
        /// Initializes a new instance of the <see cref="SignInModel"/> class.
        /// </summary>
        /// <param name="userAuthenticationservice">Service used to verify user credentials.</param>
        /// <param name="userService">Service used to look up an existing user profile by authentication ID.</param>
        public SignInModel(IUserAuthenticationService userAuthenticationservice, IUserService userService)
        {
            _userAuthenticationService = userAuthenticationservice;
            _userService = userService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignInModel"/> class.
        /// </summary>
        /// <param name="userAuthenticationservice">Service used to verify user credentials.</param>
        /// <param name="userService">Service used to look up an existing user profile by authentication ID.</param>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Gets or sets an optional local URL to redirect to after a successful sign-in.
        /// </summary>
        [BindProperty(SupportsGet = true)]// Modtag returnUrl som en parameter fra URL'en via [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        /// <summary>
        /// Represents the credentials and persistence preferences submitted on the sign-in form.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Gets or sets the user's email address or username.
            /// </summary>
            [Required]
            public string EmailOrUsername { get; set; }

            /// <summary>
            /// Gets or sets the user's plain-text password for authentication.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the authentication cookie should persist across browser sessions.
            /// </summary>
            public bool RememberMe { get; set; }
        }

        /// <summary>
        /// Handles HTTP GET requests for the sign-in page.
        /// </summary>
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
            
            if (!result.Succeeded || result.UserAuthentication is null)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Ugyldigt login.");
                return Page();
            }

            UserSummary userSummary = await _userService.GetByUserAuthIdAsync(result.UserAuthentication.Id, cancellationToken);
            if (userSummary is null)
            {
                
                List<Claim> claimsNotOnboarded = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, result.UserAuthentication.Id.ToString()), //note that the NameIdentifier is userAuthId because at this stage, the account isn't associated with a user yet!
                    new Claim(ClaimTypes.Role, result.UserAuthentication.Role.ToString()),
                    new Claim(ClaimTypes.Email, result.UserAuthentication.Email??result.UserAuthentication.Username),
                    new Claim("IsOnboarded", "false")
                };

                await SetClaimsAndSignIn(claimsNotOnboarded);

                return RedirectToPage("/Account/SignupFlow/Step3");  //if the user verified their account but did not complete registration
            }


            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,userSummary.Id.ToString()),  //note that we are using the user ID as nameIdentifier here, because this user has completed registration. 
                new Claim(ClaimTypes.Role, result.UserAuthentication.Role.ToString()),
                new Claim(ClaimTypes.Name, userSummary.DisplayName),
                new Claim("ImageId", userSummary.image?.Id.ToString()??string.Empty)
            };

            await SetClaimsAndSignIn(claims);           


            // Security check: Url.IsLocalUrl(ReturnUrl) prevents Open Redirect-attacks (ensures that it is local)
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl); // Send the user back where they came from!
            }

            if (result.UserAuthentication.Role is Role.Student) return RedirectToPage("/Feed");
            return RedirectToPage("/Account/Profile/Index");            

        }

        /// <summary>
        /// Creates a cookie authentication identity from the supplied claims and signs the user in
        /// using <see cref="InputModel.RememberMe"/> to control cookie persistence.
        /// </summary>
        /// <param name="claims">The claims to include in the user's <see cref="ClaimsIdentity"/>.</param>
        /// <returns>A task representing the asynchronous sign-in operation.</returns>
        private async Task SetClaimsAndSignIn(List<Claim> claims)
        {
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            AuthenticationProperties authProperties = new AuthenticationProperties
            {
                // IsPersistent = false turns it into a "session cookie", that is deleted, when the browser is closed.
                IsPersistent = Input.RememberMe,                
                ExpiresUtc = Input.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : null
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties); 
        }
    }
}
