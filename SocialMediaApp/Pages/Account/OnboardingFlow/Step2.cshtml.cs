using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaApp.Security;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;
using SocialMediaAppLibrary.Validation;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Pages.Account.OnboardingFlow
{
    public class Step2Model : PageModel
    {
        private readonly IStudentService _studentService;
        private readonly IUserService _userService;
        private readonly IImageService _imageService;

        public Step2Model(
            IStudentService studentService,
            IUserService userService,
            IImageService imageService)
        {
            _studentService = studentService;
            _userService = userService;
            _imageService = imageService;
        }

        public Student? Student { get; set; }
        public Image? PreviewImage { get; set; }

        [BindProperty]
        [AllowedContentTypes(["image/jpg", "image/jpeg", "image/png", "image/webp"], ErrorMessage = "Alle billeder skal være i JPG, PNG eller WebP format.")]
        [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Alle billeder må højst være 5 MB.")]
        public IFormFile? UploadedImage { get; set; }

        [BindProperty]
        [MaxLength(255, ErrorMessage = "Alternativ tekst må højst være 255 tegn.")]
        public string UserProvidedAltText { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            Student = await ResolveCurrentStudentAsync(cancellationToken);
            if (Student is null) // tjekker kun om der findes en Student i session
            {
                return RedirectToPage("/Account/SignupFlow/Step3");
            }

            if (Student.IsOnboarded)
            {
                return RedirectToPage("/Feed");
            }

            PreviewImage = HttpContext.Session.GetImage() ?? Student.Image;
            if (!string.IsNullOrEmpty(PreviewImage?.AltText))
            {
                UserProvidedAltText = PreviewImage.AltText;
            }

            return Page();
        }

        // 1) "Upload": Optimizes image in memory, stores in session for preview (no DB write)
        public async Task<IActionResult> OnPostUpload(CancellationToken cancellationToken)
        {
            Student = await ResolveCurrentStudentAsync(cancellationToken);
            if (Student is null) // sikrer at der er en aktiv Student i session
            {
                return RedirectToPage("/Account/SignupFlow/Step3");
            }

            PreviewImage = HttpContext.Session.GetImage() ?? Student.Image;

            if (UploadedImage is null || UploadedImage.Length == 0)
            {
                ModelState.AddModelError(nameof(UploadedImage), "Vælg venligst et billede.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                Image optimized = await _imageService.OptimizeImageAsync(UploadedImage!);
                optimized.AltText = GetTrimmedAltText();

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
        public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
        {
            Student = await ResolveCurrentStudentAsync(cancellationToken);
            if (Student is null) 
            {
                return RedirectToPage("/Account/SignupFlow/Step3");
            }

            Image? sessionImage = HttpContext.Session.GetImage(); // henter preview-billedet gemt under Upload
            if (sessionImage is null) // sikkerhedstjek hvis session mangler trods knappen i UI
            {
                ModelState.AddModelError(string.Empty, "Upload venligst et billede først, før du gemmer."); // viser fejl hvis der ikke er noget preview-billede
                PreviewImage = Student.Image; // bevarer eksisterende billede hvis det findes
                return Page(); // stopper videre lagring
            }

            if (!ModelState.IsValid) // validerer bl.a. UserProvidedAltText inden gem
            {
                PreviewImage = sessionImage; // viser preview igen ved valideringsfejl
                return Page(); // bliver på siden så brugeren kan rette fejlen
            }

            sessionImage.AltText = GetTrimmedAltText(); // opdaterer alt-tekst lige før gem i DB

            await SaveImageAndStudentAsync(Student, sessionImage, cancellationToken); // gemmer billede, opdaterer Student og lægger Student i session
            HttpContext.Session.Remove(SessionKeys.ImageSessionKey); // rydder midlertidigt preview-billede fra session
            return RedirectToPage("/Account/OnboardingFlow/Step3"); // sender brugeren videre til næste trin
        }

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

        private async Task SaveImageAndStudentAsync(Student student, Image imageToSave, CancellationToken cancellationToken)
        {
            Image savedImage = await _imageService.WriteImageAsync(imageToSave, cancellationToken);
            student.Image = savedImage;
            await _studentService.UpdateAsync(student.Id, student, cancellationToken);
            HttpContext.Session.SetStudent(student);
        }

        private string GetTrimmedAltText() => (UserProvidedAltText ?? string.Empty).Trim();
    }
}