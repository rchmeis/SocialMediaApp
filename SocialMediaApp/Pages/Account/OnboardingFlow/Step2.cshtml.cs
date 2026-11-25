using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaApp.Security;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;
using SocialMediaAppLibrary.Validation;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Pages.Account.OnboardingFlow
{
    /// <summary>
    /// Page model for step 2 of the student onboarding flow, handling profile image upload, previewing, and saving.
    /// </summary>
    public class Step2Model : PageModel
    {
        /// <summary>
        /// Service used to read and update <see cref="Student"/> records.
        /// </summary>
        private readonly IStudentService _studentService;

        /// <summary>
        /// Service used to optimize and persist <see cref="Image"/> records.
        /// </summary>
        private readonly IImageService _imageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Step2Model"/> class.
        /// </summary>
        /// <param name="studentService">The student service.</param>
        /// <param name="imageService">The image service.</param>
        public Step2Model(
            IStudentService studentService,
            IImageService imageService)
        {
            _studentService = studentService;
            _imageService = imageService;
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
        /// Gets or sets the uploaded image file from the form.
        /// </summary>
        [BindProperty]
        [AllowedContentTypes(["image/jpg", "image/jpeg", "image/png", "image/webp"], ErrorMessage = "Billedet skal være i JPG, PNG eller WebP format.")]
        [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Billedet må højst være 5 MB.")]
        public IFormFile? UploadedImage { get; set; }

        ///// <summary>
        ///// Gets or sets the alternative text provided by the user for the profile image.
        ///// </summary>
        //[BindProperty]
        //[MaxLength(255, ErrorMessage = "Alternativ tekst må højst være 255 tegn.")]
        //public string UserProvidedAltText { get; set; } = string.Empty;

        /// <summary>
        /// Handles GET requests by resolving the current student, enforcing onboarding state, and loading preview data.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The page result, or a redirect if the student record is missing or already onboarded.</returns>
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

            //PreviewImage = HttpContext.Session.GetImage() ?? Student.Image;
            //if (!string.IsNullOrEmpty(PreviewImage?.AltText))
            //{
            //    UserProvidedAltText = PreviewImage.AltText;
            //}

            return Page();
        }

        // 1) "Upload": Optimizes image in memory, stores in session for preview (no DB write)
        /// <summary>
        /// Handles the Upload handler by optimizing the uploaded image in memory and storing the preview in session.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The page result with the updated preview or validation errors.</returns>
        public async Task<IActionResult> OnPostUploadAsync(CancellationToken cancellationToken)
        {
            Student = await ResolveCurrentStudentAsync(cancellationToken);
            if (Student is null) 
            {
                return RedirectToPage("/Account/SignupFlow/Step3");
            }

            //PreviewImage = HttpContext.Session.GetImage() ?? Student.Image;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                Image optimized = await _imageService.OptimizeImageAsync(UploadedImage!);
                //optimized.AltText = GetTrimmedAltText();
                
                HttpContext.Session.SetImage(optimized);
                PreviewImage = optimized;
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError(nameof(UploadedImage), "Billedet kunne ikke optimeres til under 64 KB. Prøv et mindre billede.");
            }

            return Page();
        }

        // 2) "Save": Writes Image to DB, attaches to Student, updates DB & session, goes to Step 3
        /// <summary>
        /// Handles the Save handler by persisting the session preview image, updating the student, and advancing to step 3.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A redirect to step 3 on success, or the current page if validation fails or no preview image exists.</returns>
        public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
        {             
            Student = await ResolveCurrentStudentAsync(cancellationToken);
            if (Student is null) 
            {
                return RedirectToPage("/Account/SignupFlow/Step3");
            }

            Image? sessionImage = HttpContext.Session.GetImage(); 
            if (sessionImage.FileSize <=0) // sikkerhedstjek hvis session mangler trods knappen i UI
            {
                ModelState.AddModelError(string.Empty, "Upload venligst et billede først, før du gemmer."); // viser fejl hvis der ikke er noget preview-billede
                PreviewImage = Student.Image; 
                return Page(); 
            }

            if (!ModelState.IsValid) 
            {
                PreviewImage = sessionImage; 
                return Page(); 
            }

            //sessionImage.AltText = GetTrimmedAltText();

            await SaveImageAndStudentAsync(Student, sessionImage, cancellationToken); // gemmer billede, opdaterer Student og lægger Student i session
            HttpContext.Session.Remove(SessionKeys.ImageSessionKey);
            return RedirectToPage("/Account/OnboardingFlow/Step3"); 
        }

        /// <summary>
        /// Resolves the current <see cref="Student"/> from session if matching the authenticated user, or falls back to the database.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The resolved <see cref="Student"/> if found; otherwise, <see langword="null"/>.</returns>
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

        /// <summary>
        /// Saves the image to the database, assigns it to the student, updates the student record, and refreshes the session copy.
        /// </summary>
        /// <param name="student">The student receiving the profile image.</param>
        /// <param name="imageToSave">The optimized image to persist.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        private async Task SaveImageAndStudentAsync(Student student, Image imageToSave, CancellationToken cancellationToken)
        {
            Image savedImage = await _imageService.WriteImageAsync(imageToSave, cancellationToken);
            student.Image = savedImage;
            await _studentService.UpdateAsync(student.Id, student, cancellationToken);
            HttpContext.Session.SetStudent(student);
        }

        /// <summary>
        /// Returns <see cref="UserProvidedAltText"/> with leading and trailing whitespace removed.
        /// </summary>
        /// <returns>The trimmed alternative text string.</returns>
        //private string GetTrimmedAltText() => (UserProvidedAltText ?? string.Empty).Trim();
    }
}