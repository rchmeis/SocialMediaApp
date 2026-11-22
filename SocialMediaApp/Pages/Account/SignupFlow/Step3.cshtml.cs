using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using ServiceStack;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace SocialMediaApp.Pages.Account.SignupFlow
{
    /// <summary>
    /// Razor PageModel for step 3 of student registration, handling identity and campus selection,
    /// course selection, database persistence, and authentication claim updates prior to onboarding.
    /// </summary>
    public class Step3Model : PageModel
    {
        private readonly IStudentService _studentService;
        private readonly IInstitutionService _institutionService;
        private readonly IUserAuthenticationService _userAuthenticationService;

        /// <summary>
        /// Gets or sets the available institutes matching the authenticated user's email domain.
        /// </summary>
        public List<Institute> Institutes { get; set; } = new();
        /// <summary>
        /// Gets or sets the courses offered by the student's selected institute.
        /// </summary>
        public List<Course> Courses { get; set; } = new();


        /// <summary>
        /// Initializes a new instance of the <see cref="Step3Model"/> class.
        /// </summary>
        /// <param name="studentService">Service used to persist student and user records.</param>
        /// <param name="institutionService">Service used to resolve email domains, institutes, and courses.</param>
        /// <param name="userauthenticationService">Service used for user authentication operations.</param>
        public Step3Model(IStudentService studentService, IInstitutionService institutionService, IUserAuthenticationService userauthenticationService)
        {
            _studentService = studentService;
            _institutionService = institutionService;
            _userAuthenticationService = userauthenticationService;
        }

        /// <summary>
        /// Gets or sets the form input values submitted across step 3 sub-steps.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new();

        /// <summary>
        /// Represents the input fields collected during step 3 of the student signup flow.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Gets or sets the student's first name.
            /// </summary>
            [Required]
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the student's last name.
            /// </summary>
            [Required]
            public string LastName { get; set; }= string.Empty;

            /// <summary>
            /// Gets or sets the identifier of the selected institute.
            /// </summary>
            [Required]
            public int InstituteId { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the selected course.
            /// </summary>
            public int CourseId { get; set; }
            
        }

        /// <summary>
        /// Handles HTTP GET requests for step 3, loading course selection if an institute is already
        /// stored in session or loading matching institutes for the user's email domain.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
        /// <returns>An <see cref="IActionResult"/> rendering the page or redirecting to step 1 if no domain is found.</returns>
        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            Student? student = HttpContext.Session.GetStudent();
            if (student is not null && student.Institute is not null)
            {
                await LoadCourseSelectionAsync(student, cancellationToken);
                return Page();
            }

            string? domainName = GetEmailDomain();          
            if (domainName.IsNullOrEmpty())
            {
                return RedirectToPage("/Account/SignupFlow/Step1");
            }
            Institutes = await _institutionService.GetByDomainAsync(domainName, cancellationToken);   
            return Page();
        }


        // Sub-step A: Validates name + institute and stores them in session before loading courses.
        /// <summary>
        /// Handles the POST action for sub-step A by validating name and institute selection,
        /// saving the partial <see cref="Student"/> in session, and loading courses for that institute.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
        /// <returns>An <see cref="IActionResult"/> re-rendering the page with courses or validation errors, or redirecting to step 1.</returns>
        public async Task<IActionResult> OnPostSelectInstituteAsync(CancellationToken cancellationToken)
        {
            string? domainName = GetEmailDomain();

            if (domainName.IsNullOrEmpty())
            {
                return RedirectToPage("/Account/SignupFlow/Step1");
            }

            if (string.IsNullOrWhiteSpace(Input.Name))
            {
                ModelState.AddModelError(nameof(Input.Name), "Fornavn er påkrævet.");
            }
            if (string.IsNullOrWhiteSpace(Input.LastName))
            {
                ModelState.AddModelError(nameof(Input.LastName), "Efternavn er påkrævet.");
            }
            if (Input.InstituteId <= 0)
            {
                ModelState.AddModelError(nameof(Input.InstituteId), "Vælg venligst et institut.");
            }

            Institutes = await _institutionService.GetByDomainAsync(domainName, cancellationToken); //populating the list

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Institute? institute = Institutes.FirstOrDefault(i => i.Id == Input.InstituteId);
            if (institute is null)
            {
                ModelState.AddModelError(nameof(Input.InstituteId), "Det valgte institut er ikke gyldigt.");
                return Page();
            }

            Student student = HttpContext.Session.GetStudent() ?? new Student();
            student.FirstName = Input.Name.Trim();
            student.LastName = Input.LastName.Trim();
            student.Institute = institute;
            student.Course = null;

            HttpContext.Session.SetStudent(student);
            await LoadCourseSelectionAsync(student, cancellationToken);
            return Page();
        }

        // Sub-step B: Validates the chosen course, persists the Student, refreshes claims, and moves to onboarding.
        /// <summary>
        /// Handles the POST action for sub-step B by validating the selected course for the student's
        /// institute, writing the <see cref="Student"/> to the database, updating session and auth claims,
        /// and redirecting to the onboarding flow.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
        /// <returns>An <see cref="IActionResult"/> redirecting to onboarding step 1, or re-rendering with errors.</returns>
        public async Task<IActionResult> OnPostSelectCourseAsync(CancellationToken cancellationToken)
        {
            Student? existingStudent = HttpContext.Session.GetStudent();
            if (existingStudent is null || existingStudent.Institute is null)
            {
                string? domainName = GetEmailDomain();
                if (string.IsNullOrWhiteSpace(domainName))
                {
                    return RedirectToPage("/Account/SignupFlow/Step1");
                }

                Institutes = await _institutionService.GetByDomainAsync(domainName, cancellationToken);
                return Page();
            }

            if (Input.CourseId <= 0)
                {
                    ModelState.AddModelError(nameof(Input.CourseId), "Vælg venligst en uddannelse.");
                    await LoadCourseSelectionAsync(existingStudent, cancellationToken);
                    return Page();
                }

                Course? course = await _institutionService.GetCourseForInstituteAsync(
                    Input.CourseId,
                    existingStudent.Institute.Id,
                    cancellationToken);

                if (course is null)
                {
                    ModelState.AddModelError(nameof(Input.CourseId), "Den valgte uddannelse hører ikke til dit institut.");
                    await LoadCourseSelectionAsync(existingStudent, cancellationToken);
                    return Page();
               }

                string? nameIdentifierValue = User.FindFirstValue(ClaimTypes.NameIdentifier);  
                if (!int.TryParse(nameIdentifierValue, out int userAuthId))
                {
                    throw new InvalidOperationException("Missing or invalid NameIdentifier claim.");
                }

                existingStudent.Course = course;
                existingStudent.IsOnboarded = false;
                existingStudent.UserAuthenticationId = userAuthId;


                existingStudent= await _studentService.WriteAsync(existingStudent, cancellationToken); //upon writing the student in the database, we get an updated student returned that includes the autogenerated user.id!
                HttpContext.Session.SetStudent(existingStudent);

                await UpdateClaimsAsync(existingStudent);                

                return RedirectToPage("/Account/OnboardingFlow/Step1");
            }

        /// <summary>
        /// Handles the POST action to return the student to institute selection and clear
        /// previously selected institute and course values from session.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
        /// <returns>An <see cref="IActionResult"/> re-rendering institute selection or redirecting to step 1.</returns>
        public async Task<IActionResult> OnPostChangeInstituteAsync(CancellationToken cancellationToken)
            {
                string? domainName = GetEmailDomain();
                if (string.IsNullOrWhiteSpace(domainName))
                {
                    return RedirectToPage("/Account/SignupFlow/Step1");
                }

                Student? sessionStudent = HttpContext.Session.GetStudent();
                if (sessionStudent is not null)
                {
                    Input.Name = sessionStudent.FirstName;
                    Input.LastName = sessionStudent.LastName;
                    Input.InstituteId = sessionStudent.Institute?.Id ?? 0;

                    sessionStudent.Institute = null;
                    sessionStudent.Course = null;
                    HttpContext.Session.SetStudent(sessionStudent);
                }

                Institutes = await _institutionService.GetByDomainAsync(domainName, cancellationToken);
                return Page();
            }

        /// <summary>
        /// Extracts the email domain from the current user's <see cref="ClaimTypes.Email"/> claim.
        /// </summary>
        /// <returns>The extracted domain name, or <see langword="null"/> if no email claim is present.</returns>
        private string? GetEmailDomain()
            {
                string? email = User.FindFirstValue(ClaimTypes.Email);
                return string.IsNullOrWhiteSpace(email)
                    ? null
                    : _institutionService.ExtractDomain(email);
            }

        /// <summary>
        /// Populates <see cref="Input"/> with the student's saved data and loads courses for their selected institute.
        /// </summary>
        /// <param name="student">The student session state containing a non-null <see cref="Student.Institute"/>.</param>
        /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
        private async Task LoadCourseSelectionAsync(Student student, CancellationToken cancellationToken)
            {
                Input.Name = student.FirstName;
                Input.LastName = student.LastName;
                Input.InstituteId = student.Institute!.Id;
                Input.CourseId = student.Course?.Id ?? 0;

                Courses = await _institutionService.GetCoursesByInstituteIdAsync(
                    student.Institute.Id,
                    cancellationToken);
            }

        /// <summary>
        /// Refreshes the cookie authentication identity after registration so <see cref="ClaimTypes.NameIdentifier"/>
        /// reflects the newly created domain user ID while preserving cookie persistence settings.
        /// </summary>
        /// <param name="student">The persisted student entity containing the generated domain user ID and name.</param>
        public async Task UpdateClaimsAsync(Student student)
            {
                var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                var existingProps = authResult.Properties;

                string fullName = $"{student.FirstName.Trim()} {student.LastName.Trim()}".Trim();
                string role = User.FindFirstValue(ClaimTypes.Role) ?? Role.Student.ToString();

                var updatedClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, student.Id.ToString()), // now Domain user ID instead UserAuth ID
                    new Claim(ClaimTypes.Name, fullName),
                    new Claim(ClaimTypes.Role, role),    
                };

                var updatedIdentity = new ClaimsIdentity(
                    updatedClaims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                var updatedProperties = new AuthenticationProperties
                {
                    IsPersistent = existingProps?.IsPersistent ?? false,
                    ExpiresUtc = existingProps?.ExpiresUtc
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(updatedIdentity),
                    updatedProperties);
            }
    }
}
