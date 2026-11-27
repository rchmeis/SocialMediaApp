using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaApp.Security;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SocialMediaApp.Pages.Account.OnboardingFlow
{
    /// <summary>
    /// Page model for step 1 of the student onboarding flow, collecting the student's bio and selected interests.
    /// </summary>
    public class Step1Model : PageModel
    {

        private readonly IInterestService _interestService;
        private readonly IStudentService _studentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Step1Model"/> class with the required services.
        /// </summary>
        /// <param name="interestService">Service used to retrieve available interests.</param>
        /// <param name="studentService">Service used to retrieve and persist <see cref="Student"/> records.</param>
        public Step1Model(IInterestService interestService, IStudentService studentService)
        {
            _interestService = interestService;
            _studentService = studentService;
        }

        /// <summary>
        /// Gets or sets the current <see cref="Student"/> being onboarded.
        /// </summary>
        public Student Student { get; set; }

        /// <summary>
        /// Gets or sets the list of available <see cref="Interest"/> items shown on the page.
        /// </summary>
        public List<Interest>? allInterests { get; set; } = new();

        /// <summary>
        /// Gets or sets the bio text bound from the onboarding step 1 form.
        /// </summary>
        [BindProperty]
        [StringLength(255, ErrorMessage = "Din bio må højst være 255 tegn.")]
        public string? InputBio { get; set; }

        /// <summary>
        /// Gets or sets the IDs of interests selected by the student on step 1.
        /// </summary>
        [BindProperty]
        public List<int> SelectedInterestIds { get; set; } = new();

        /// <summary>
        /// Handles the GET request for onboarding step 1 by checking whether the user is already onboarded,
        /// resolving the current <see cref="Student"/> from session or database, loading available interests,
        /// and pre-filling previously entered bio and interest selections.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests while resolving the student.</param>
        /// <returns>
        /// A redirect to <c>/Feed</c> if already onboarded, <c>/Account/SignupFlow/Step3</c> if no student record is found,
        /// or the step 1 page with pre-populated fields.
        /// </returns>
        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            if (User.FindFirstValue("IsOnboarded") == "true")
                return RedirectToPage("/Feed");

            Student = await ResolveCurrentStudentAsync(cancellationToken);  //extra check hvis feks brugeren lukkede browseren efter step3 eller lod tab være åben 20min+ og session derfor er tom, men student er gemt og derfor IKKE skal udfylde navn efternavn osv igen
            if (Student is null)
            {
                TempData["Error"] = "Kunne ikke fortsætte med onboarding, prøv igen";
                return RedirectToPage("/Account/SignupFlow/Step3");
            }

            if (Student.IsOnboarded)
                return base.RedirectToPage("/Feed");

            LoadInterests(cancellationToken);

            // Pre-fill if the user comes back to this step
            InputBio = Student.Bio;
            SelectedInterestIds = allInterests
                .Where(i => Student.Interests.Contains(i.Name))
                .Select(i => i.Id)
                .ToList();
            
            return Page();
        }

        /// <summary>
        /// Handles the POST submission for onboarding step 1 by validating the user's onboarding state,
        /// bio, and selected interests, updating the <see cref="Student"/> in session, and advancing to step 2.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests while resolving the student.</param>
        /// <returns>
        /// A redirect to <c>/Feed</c> if already onboarded, <c>/Account/SignupFlow/Step3</c> if no student record is found,
        /// <c>/Account/OnboardingFlow/Step2</c> on valid submission, or the current page if validation fails.
        /// </returns>
        public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
        {
            if (User.FindFirstValue("IsOnboarded") == "true")
                return RedirectToPage("/Feed");

            Student = await ResolveCurrentStudentAsync(cancellationToken);
            if (Student is null)
            {
                TempData["Error"] = "Kunne ikke fortsætte med onboarding, prøv igen";
                return RedirectToPage("/Account/SignupFlow/Step3");
            }

            if (Student.IsOnboarded)
                return RedirectToPage("/Feed");

            LoadInterests(cancellationToken);  
            
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
            
            Student.Bio = InputBio?.Trim() ?? string.Empty;
            Student.Interests = allInterests
                .Where(i => SelectedInterestIds.Contains(i.Id))
                .Select(i => i.Name)
                .ToList();
            
            HttpContext.Session.SetStudent(Student);
            
            return RedirectToPage("/Account/OnboardingFlow/Step2");
        }

        //Denne metode henter student enten fra session eller fra databasen hvis session afleverer et tomt student object, checkes ved at id ikke svarer til User.id
        /// <summary>
        /// Resolves the <see cref="Student"/> for the currently authenticated user, checking the active session first
        /// and falling back to the database if the session is empty or belongs to a different user.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during database access.</param>
        /// <returns>
        /// The matching <see cref="Student"/> from the session or database, or <see langword="null"/> if no student record exists.
        /// </returns>
        private async Task<Student?> ResolveCurrentStudentAsync(CancellationToken cancellationToken)
        {
            int userId = User.GetUserId();
            Student sessionStudent = HttpContext.Session.GetStudent();

            if (sessionStudent.Id == userId)
            {
                return sessionStudent;
            }

            Student? studentFromDb = await _studentService.ReadByIdAsync(userId, cancellationToken);
            if (studentFromDb is not null)
            {
                HttpContext.Session.SetStudent(studentFromDb);
            }

            return studentFromDb;
        }

        //OBS! der er allerede en extension method User.GetUserId(); som gør det her. metoden er udkommenteret.

        // Session holds the onboarding answers; the id comes from the login cookie.  
        //private Student GetOnboardingStudent()
        //{
        //    Student Student = HttpContext.Session.GetStudent();

        //    if (Student.Id == 0 && int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id))
        //    {
        //        Student.Id = id;
        //    }

        //    return Student;
        //}

        /// <summary>
        /// Loads all available <see cref="Interest"/> items into <see cref="allInterests"/>,
        /// setting a <see cref="PageModel.TempData"/> error message if retrieval fails.
        /// </summary>
        public async Task LoadInterests(CancellationToken cancellationToken)
        {
            try
            {
                allInterests = await _interestService.ReadAllAsync(cancellationToken);
            }
            catch
            {
                TempData["Error"] = "Something went wrong getting all interests, try again later";
            }
        }
    }
}