using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services;
using SocialMediaAppLibrary.Services.Interfaces;
using SocialMediaAppLibrary.Validation;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Pages.Account.SignupFlow
{
    /// <summary>
    /// PageModel for the first step of student account registration, handling credential input,
    /// institution domain validation, duplicate account checks, password hashing, and session handoff.
    /// </summary>
    public class Step1Model : PageModel
    {
        private readonly IUserAuthenticationService _userAuthenticationService;
        private readonly IInstitutionService _institutionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Step1Model"/> class.
        /// </summary>
        /// <param name="userAuthenticationService">Service used for duplicate checks and password hashing.</param>
        /// <param name="institutionService">Service used to retrieve allowed student email domains.</param>
        public Step1Model(IUserAuthenticationService userAuthenticationService, IInstitutionService institutionService)
        {
            _userAuthenticationService = userAuthenticationService;
            _institutionService = institutionService;
        }

        /// <summary>
        /// Gets or sets the bound form input data submitted in Step 1.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new();

        /// <summary>
        /// Represents the form fields collected during the first registration step.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Gets or sets the student's institutional email address.
            /// </summary>
            [Required(ErrorMessage = "Indtast din studiemail")]
            [EmailAddress(ErrorMessage = AccountValidationRules.EmailInvalidMessage)]
            [StringLength(AccountValidationRules.EmailMaxLength,
            ErrorMessage = "E-mailadressen er for lang.")]

            public string Email { get; set; } = string.Empty;


            /// <summary>
            /// Gets or sets the desired account username.
            /// </summary>
            [Required(ErrorMessage = "Vælg et brugernavn")]
            [StringLength(AccountValidationRules.UsernameMaxLength,
            MinimumLength = AccountValidationRules.UsernameMinLength,
            ErrorMessage = AccountValidationRules.UsernameErrorMessage)]
            [RegularExpression(AccountValidationRules.UsernamePattern,
            ErrorMessage = AccountValidationRules.UsernameErrorMessage)]

            public string Username { get; set; } = string.Empty;


            /// <summary>
            /// Gets or sets the chosen plain-text password prior to hashing.
            /// </summary>
            [Required(ErrorMessage = "Vælg en adgangskode.")]
            [DataType(DataType.Password)]
            [StringLength(AccountValidationRules.PasswordMaxLength,
            MinimumLength = AccountValidationRules.PasswordMinLength,
            ErrorMessage = AccountValidationRules.PasswordErrorMessage)]
            [RegularExpression(AccountValidationRules.PasswordPattern,
            ErrorMessage = AccountValidationRules.PasswordErrorMessage)]

            public string Password { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the confirmation password that must match <see cref="Password"/>.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string RepeatPassword { get; set; } = string.Empty;
      
        }

        /// <summary>
        /// Handles HTTP GET requests to display the Step 1 registration form.
        /// </summary>
        public void OnGet()
        {
        }

        /// <summary>
        /// Handles HTTP POST requests for Step 1 registration by validating inputs, checking
        /// allowed student email domains and existing accounts, hashing the password, storing the
        /// pending <see cref="UserAuthentication"/> in session, and redirecting to Step 2.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A redirect to <c>/Account/SignupFlow/Step2</c> when validation succeeds, or the current
        /// page with <see cref="PageModel.ModelState"/> errors if any check fails.
        /// </returns>
        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken=default)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // explicit check with custom errormessages using helper methods
            if (!AccountValidationRules.IsValidUsername(Input.Username))
            {
                ModelState.AddModelError(
                    $"{nameof(Input)}.{nameof(Input.Username)}",
                    AccountValidationRules.UsernameErrorMessage);
            }

            if (!AccountValidationRules.IsValidPassword(Input.Password))
            {
                ModelState.AddModelError(
                    $"{nameof(Input)}.{nameof(Input.Password)}",
                    AccountValidationRules.PasswordErrorMessage);
            }

            if (Input.Password != Input.RepeatPassword)
            {
                ModelState.AddModelError(
                    $"{nameof(Input)}.{nameof(Input.RepeatPassword)}",
                    "Adgangskoderne er ikke ens.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Fetch the allowed institution domains from the database on POST
            List<string> allowedDomains = await _institutionService.ReadAllDomainsAsync(cancellationToken);

            if (!AccountValidationRules.HasAllowedStudentDomain(Input.Email, allowedDomains))
            {
                ModelState.AddModelError(
                    $"{nameof(Input)}.{nameof(Input.Email)}",
                    AccountValidationRules.StudentEmailDomainErrorMessage);
                return Page();
            }

         
            // Normalize inputs before saving/passing along
            string normalizedEmail = Input.Email.Trim().ToLowerInvariant();
            string normalizedUsername = Input.Username.Trim();


            //Check if the account already exists (checks by email AND username)
            DuplicateAccountCheck duplicates = await _userAuthenticationService.CheckDuplicateAccountAsync(normalizedEmail,
                    normalizedUsername, cancellationToken);

            if (duplicates.EmailExists)
            {
                ModelState.AddModelError(
                    $"{nameof(Input)}.{nameof(Input.Email)}",
                    "Denne e-mailadresse er allerede i brug.");
            }

            if (duplicates.UsernameExists)
            {
                ModelState.AddModelError(
                    $"{nameof(Input)}.{nameof(Input.Username)}",
                    "Dette brugernavn er allerede optaget.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }


            UserAuthentication userAuth = new UserAuthentication
            {
                Email = normalizedEmail,
                Username = normalizedUsername,
                Role = Role.Student,
                IsVerified = false
            };
            userAuth.PasswordHashed = _userAuthenticationService.HashPassword(userAuth, Input.Password);

            HttpContext.Session.SetUserAuthentication(userAuth);

            return RedirectToPage("/Account/SignupFlow/Step2");
        }
    }
}
