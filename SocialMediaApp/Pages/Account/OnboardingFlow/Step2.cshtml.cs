using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaApp.Security;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;
using SocialMediaAppLibrary.Validation;
using System.Security.Claims;

namespace SocialMediaApp.Pages.Account.OnboardingFlow
{
    /// <summary>
    /// Page model for step 2 of the student onboarding flow, handling profile image selection,
    /// in-memory previewing via session, saving to the database, or skipping to step 3.
    /// </summary>
    public class Step2Model : PageModel
    {
        private readonly IStudentService _studentService;
        private readonly IImageService _imageService;
        private readonly ILogger<Step2Model> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Step2Model"/> class.
        /// </summary>
        /// <param name="studentService">Service used to read and update <see cref="Student"/> records.</param>
        /// <param name="imageService">Service used to optimize and persist <see cref="Image"/> records.</param>
        /// <param name="logger">Logger used to record diagnostic and error messages during onboarding step 2.</param>
        public Step2Model(
            IStudentService studentService,
            IImageService imageService,
            ILogger<Step2Model> logger)
        {
            _studentService = studentService;
            _imageService = imageService;
            _logger = logger;
        }

        /// <summary>
        /// Gets or sets the current <see cref="Student"/> being onboarded.
        /// </summary>
        public Student? Student { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Image"/> shown in the preview area.
        /// </summary>
        public Image? PreviewImage { get; set; }

        /// <summary>
        /// Gets or sets the name of the file selected by the user in the file input.
        /// </summary>
        public string? SelectedFileName { get; set; }

        /// <summary>
        /// Gets or sets the uploaded image file from the form.
        /// </summary>
        [BindProperty]
        [AllowedContentTypes(["image/jpg", "image/jpeg", "image/png", "image/webp"], ErrorMessage = "Billedet skal være i JPG, PNG eller WebP format.")]
        [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Billedet må højst være 5 MB.")]
        public IFormFile? UploadedImage { get; set; }

        /// <summary>
        /// Handles GET requests by resolving the current student, enforcing onboarding state,
        /// and loading any existing session or saved profile image into the preview.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
        /// <returns>The page result, or a redirect if the student is missing or already onboarded.</returns>
        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            Student = await ResolveCurrentStudentAsync(cancellationToken);
            if (Student is null)
            {
                return RedirectToPage("/Account/SignupFlow/Step3");
            }

            if (Student.IsOnboarded)
            {
                return RedirectToPage("/Feed");
            }

            PreviewImage = ResolvePreviewImage(Student);
            return Page();
        }

        /// <summary>
        /// Handles the Upload/Preview action by optimizing the selected image in memory
        /// and storing it in session so the user can preview it before saving to the database.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
        /// <returns>The page result with the updated preview or validation errors.</returns>
        public async Task<IActionResult> OnPostUploadAsync(CancellationToken cancellationToken)
        {
            Student = await ResolveCurrentStudentAsync(cancellationToken);
            if (Student is null)
            {
                return RedirectToPage("/Account/SignupFlow/Step3");
            }

            PreviewImage = ResolvePreviewImage(Student);

            if (UploadedImage is null)
            {
                ModelState.AddModelError(nameof(UploadedImage), "Vælg venligst et billede først.");
                return Page();
            }

            SelectedFileName = UploadedImage.FileName;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                Image optimized = await _imageService.OptimizeImageAsync(UploadedImage);
                HttpContext.Session.SetImage(optimized);
                PreviewImage = optimized;
            }
            catch (OperationCanceledException)
            {
                ModelState.AddModelError(string.Empty, "Handlingen blev annulleret.");
            }
            catch (InvalidOperationException)
            {
                // Input/file constraint: image could not be compressed within the 64 KB limit.
                ModelState.AddModelError(nameof(UploadedImage), "Billedet kunne ikke optimeres til under 64 KB. Vælg et mindre eller enklere billede.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to optimize uploaded image in Onboarding Step 2 for UserId {UserId}",
                    Student.Id);

                ModelState.AddModelError(
                    string.Empty,
                    "Kunne ikke behandle billedet - prøv igen eller kontakt systemadministratoren.");
            }

            return Page();
        }

        /// <summary>
        /// Handles the Save action by persisting the previewed image from session to the database,
        /// attaching it to the <see cref="Student"/>, clearing the session preview, and navigating to step 3.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
        /// <returns>A redirect to step 3 on success, or the current page if no preview image is available or saving fails.</returns>
        public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
        {
            Student = await ResolveCurrentStudentAsync(cancellationToken);
            if (Student is null)
            {
                return RedirectToPage("/Account/SignupFlow/Step3");
            }

            Image? sessionImage = HttpContext.Session.GetImage();
            if (!HasValidImageData(sessionImage))
            {
                ModelState.AddModelError(string.Empty, "Vælg et billede og klik på Forhåndsvis, før du gemmer.");
                PreviewImage = ResolvePreviewImage(Student);
                return Page();
            }

            string? claimName = User.FindFirstValue(ClaimTypes.Name);
            sessionImage!.AltText = !string.IsNullOrWhiteSpace(claimName)
                ? $"Profilbillede for {claimName}"
                : $"Profilbillede for {Student.FirstName} {Student.LastName}".Trim();

            bool previousOnboardedState = Student.IsOnboarded;
            Student.IsOnboarded = true;

            bool saved = await TrySaveImageAndStudentAsync(Student, sessionImage, cancellationToken);
            if (!saved)
            {
                Student.IsOnboarded = previousOnboardedState;
                ModelState.AddModelError(
                    string.Empty,
                    "Kunne ikke opdatere din profil - prøv igen eller kontakt systemadministratoren.");
                PreviewImage = sessionImage;
                return Page();
            }

            HttpContext.Session.Remove(SessionKeys.ImageSessionKey);
            return RedirectToPage("/Account/OnboardingFlow/Step3");
        }

        /// <summary>
        /// Handles the Skip action by discarding any temporary preview image in session
        /// and proceeding to step 3 without saving a new profile image.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
        /// <returns>A redirect to step 3, or to signup step 3 if no student is resolved.</returns>
        public async Task<IActionResult> OnPostSkipAsync(CancellationToken cancellationToken)
        {
            Student = await ResolveCurrentStudentAsync(cancellationToken);
            if (Student is null)
            {
                return RedirectToPage("/Account/SignupFlow/Step3");
            }

            try
            {
                if (!string.IsNullOrEmpty(Student.Bio) || Student.Interests.Count > 0)
                {
                    Student? updatedStudent = await _studentService.UpdateAsync(ClaimsPrincipalExtensions.GetUserId(User), Student, cancellationToken);
                    if (updatedStudent is not null)
                    {
                        Student = updatedStudent;
                    }

                    HttpContext.Session.SetStudent(Student);
                }              

                HttpContext.Session.Remove(SessionKeys.ImageSessionKey);
                return RedirectToPage("/Account/OnboardingFlow/Step3");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed during Skip in Onboarding Step 2");

                ModelState.AddModelError(
                    string.Empty,
                    "Kunne ikke gemme dine oplysninger - prøv igen eller kontakt systemadministratoren.");
                if (Student is not null)
                {
                    PreviewImage = ResolvePreviewImage(Student);
                }
                return Page();
            }                
        }

        /// <summary>
        /// Resolves the current <see cref="Student"/> from session if matching the authenticated user,
        /// or falls back to loading it from <see cref="IStudentService"/>.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
        /// <returns>The resolved <see cref="Student"/> if found; otherwise <see langword="null"/>.</returns>
        private async Task<Student?> ResolveCurrentStudentAsync(CancellationToken cancellationToken)
        {
            int userId = User.GetUserId();
            Student? sessionStudent = HttpContext.Session.GetStudent();

            if (sessionStudent is not null && sessionStudent.Id == userId)
            {
                return sessionStudent;
            }

            try
            {
                Student? studentFromDb = await _studentService.ReadByIdAsync(userId, cancellationToken);
                if (studentFromDb is not null)
                {
                    HttpContext.Session.SetStudent(studentFromDb);
                }

                return studentFromDb;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Loading student in Onboarding Step 2 was cancelled for UserId {UserId}", userId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to load student in Onboarding Step 2 for UserId {UserId}",
                    userId);

                return null;
            }
        }

        /// <summary>
        /// Resolves the best available preview <see cref="Image"/> from session or the student's existing image.
        /// </summary>
        /// <param name="student">The current student.</param>
        /// <returns>A preview <see cref="Image"/> with binary data if available; otherwise <see langword="null"/>.</returns>
        private Image? ResolvePreviewImage(Student student)
        {
            Image? sessionImage = HttpContext.Session.GetImage();
            if (HasValidImageData(sessionImage))
            {
                return sessionImage;
            }

            if (HasValidImageData(student.Image))
            {
                return student.Image;
            }

            return null;
        }

        /// <summary>
        /// Persists the optimized image to the database, links it to the student, updates the student record,
        /// and refreshes the student stored in session.
        /// </summary>
        /// <param name="student">The student receiving the profile image.</param>
        /// <param name="imageToSave">The optimized image from session to persist.</param>
        /// <param name="cancellationToken">Token used to cancel asynchronous operations.</param>
        /// <returns><see langword="true"/> if both image and student were persisted; otherwise <see langword="false"/>.</returns>
        private async Task<bool> TrySaveImageAndStudentAsync(Student student, Image imageToSave, CancellationToken cancellationToken)
        {
            try
            {
                Image savedImage = await _imageService.WriteImageAsync(imageToSave, cancellationToken);
                student.Image = savedImage;

                Student? updatedStudent = await _studentService.UpdateAsync(student.Id, student, cancellationToken);
                if (updatedStudent is null)
                {
                    _logger.LogWarning(
                        "Student update returned null in Onboarding Step 2 for StudentId {StudentId}",
                        student.Id);
                    return false;
                }

                HttpContext.Session.SetStudent(updatedStudent);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to save profile image and update Student in Onboarding Step 2 for StudentId {StudentId}",
                    student.Id);
                return false;
            }
        }

        /// <summary>
        /// Checks whether an <see cref="Image"/> instance contains non-empty binary data and a positive file size.
        /// </summary>
        /// <param name="image">The image to validate.</param>
        /// <returns><see langword="true"/> if the image has binary data; otherwise <see langword="false"/>.</returns>
        private static bool HasValidImageData(Image? image) =>
            image is not null &&
            image.ImageData is { Length: > 0 } &&
            image.FileSize > 0;
    }
}