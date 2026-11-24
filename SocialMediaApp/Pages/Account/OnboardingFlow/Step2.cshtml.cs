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
        [AllowedContentTypes(["image/jpg", "image/jpeg", "image/png", "image/webp"],ErrorMessage = "Alle billeder skal være i JPG, PNG eller WebP format.")]
        [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Alle billeder må højst være 5 MB.")]
        public IFormFile? UploadedImage { get; set; }

        [BindProperty]
        [MaxLength(255, ErrorMessage = "Alternativ tekst må højst være 255 tegn.")]
        public string UserProvidedAltText { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            Student = HttpContext.Session.GetStudent();
            if (Student is null)
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
        public async Task<IActionResult> OnPostUpload()
        {
            Student = HttpContext.Session.GetStudent();
            if (Student is null)
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
                ModelState.AddModelError(nameof(UploadedImage),"Billedet kunne ikke optimeres til under 64 KB. Prøv et mindre billede.");
            }

            return Page();
        }

        // 2) "Save": Writes Image to DB, attaches to Student, updates DB & session, goes to Step 3
        public async Task<IActionResult> OnPostSave(CancellationToken cancellationToken)
        {
            Student = HttpContext.Session.GetStudent();
            if (Student is null)
            {
                return RedirectToPage("/Account/SignupFlow/Step3");
            }

            string altText = GetTrimmedAltText();
            Image? sessionImage = HttpContext.Session.GetImage();

            // User picked a file and clicked Save directly without clicking Upload first
            if (UploadedImage is not null && UploadedImage.Length > 0)
            {
                if (!ModelState.IsValid)
                {
                    PreviewImage = sessionImage ?? Student.Image;
                    return Page();
                }

                try
                {
                    Image optimized = await _imageService.OptimizeImageAsync(UploadedImage);
                    optimized.AltText = altText;

                    await SaveImageAndStudentAsync(Student, optimized, cancellationToken); //saves both in DB and saves Student in session
                    HttpContext.Session.Remove(SessionKeys.ImageSessionKey);
                    return RedirectToPage("/Account/OnboardingFlow/Step3");
                }
                catch (InvalidOperationException)
                {
                    ModelState.AddModelError(nameof(UploadedImage),
                        "Billedet kunne ikke optimeres til under 64 KB. Prøv et mindre billede.");
                    PreviewImage = sessionImage ?? Student.Image;
                    return Page();
                }
            }

            // User clicked Upload first (so preview is in session) and now clicks Save
            if (sessionImage is not null)
            {
                if (!ModelState.IsValid)
                {
                    PreviewImage = sessionImage;
                    return Page();
                }

                sessionImage.AltText = altText;

                await SaveImageAndStudentAsync(Student, sessionImage, cancellationToken);
                HttpContext.Session.Remove(SessionKeys.ImageSessionKey);
                return RedirectToPage("/Account/OnboardingFlow/Step3");
            }

            ModelState.AddModelError(string.Empty, "Upload venligst et billede først, før du gemmer.");
            PreviewImage = Student.Image;
            return Page();
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