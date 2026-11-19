//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using SocialMediaAppLibrary.Models;
//using SocialMediaAppLibrary.Services.Interfaces;
//using System.ComponentModel;
//using System.ComponentModel.DataAnnotations;
//using System.Security.Claims;

//namespace SocialMediaApp.Pages.Account
//{
//    public class SignInModel : PageModel
//    {
//        private readonly IUserAuthenticationService _userAuthenticationService;
//        private readonly IUserService _userService;

//        public SignInModel(IUserAuthenticationService userAuthenticationservice, IUserService userService)
//        {
//            _userAuthenticationService = userAuthenticationservice;
//            _userService = userService;
//        }

//        [BindProperty]
//        public InputModel Input { get; set;}

//        // Modtag returnUrl som en parameter fra URL'en via [BindProperty(SupportsGet = true)]
//        [BindProperty(SupportsGet = true)]
//        public string? ReturnUrl { get; set; }


//        public class InputModel
//        {
//            [Required]        
//            public string EmailOrUsername { get; set; }

//            [Required]
//            [DataType(DataType.Password)]          
//            public string Password { get; set; }

//            public bool RememberMe { get; set; }
//        }
       

//        public void OnGet()
//        {
//        }

//        /// <summary>
//        /// Handles the HTTP POST request for user login. 
//        /// Validates credentials, sets up persistent or session-bound cookie authentication based on 
//        /// the "Remember Me" option, and redirects the user safely back to their original destination 
//        /// (ReturnUrl) or to a default role-specific page.
//        /// </summary>
//        /// <returns>
//        /// A task that represents the asynchronous operation. The task result contains 
//        /// an <see cref="IActionResult"/> redirect result, or re-renders the page with validation errors if login fails.
//        /// </returns>
//        public async Task<IActionResult> OnPostAsync()
//        {
//            if (!ModelState.IsValid) return Page();

//            UserAuthentication userAuth = await _userAuthenticationService.AuthenticateAsync(Input.EmailOrUsername, Input.Password);
//            if (userAuth is null || userAuth.DeletedAt is null)
//            {
//                ModelState.AddModelError(string.Empty, "Ugyldigt login.");
//                return Page();
//            }

//            UserSummary userSummary = _userService.GetByUserAuthId(userAuth.Id);

//            List<Claim> claims = new List<Claim>
//            {
//                new Claim(ClaimTypes.NameIdentifier, userAuth.Id.ToString()),
//                new Claim(ClaimTypes.Role, userAuth.Role.ToString()),
//                new Claim(ClaimTypes.Name, userSummary.DisplayName),
//                new Claim("ImageId", userSummary.Id.ToString())
//            };

//            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

//            AuthenticationProperties authProperties = new AuthenticationProperties
//            {
//                // IsPersistent = true means that the cookie is saved in the harddisk and survives even if the browser is closed.
//                // IsPersistent = false turns it into a "session cookie", that is deleted, when the browser is closed.
//                IsPersistent = Input.RememberMe,

//                // If "Remember me" is ticked the cookie will expire in 14 days. Otherwise it is set to null and will only survive the session.
//                ExpiresUtc = Input.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : null
//            };

//            await HttpContext.SignInAsync(
//                CookieAuthenticationDefaults.AuthenticationScheme, 
//                new ClaimsPrincipal(claimsIdentity), 
//                authProperties);  //NOTICE! authProperties added!


//            // Sikkerhedstjek: Url.IsLocalUrl(ReturnUrl) forhindrer Open Redirect-angreb (sikrer, at den er lokal)
//            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
//            {
//                return LocalRedirect(ReturnUrl); // Send brugeren tilbage hvor de kom fra!
//            }

//            if (userAuth.Role is Role.Student) return RedirectToPage("/Feed");
//            return RedirectToPage("/Account/Profile/Index");         

//        }
//    }
//}
