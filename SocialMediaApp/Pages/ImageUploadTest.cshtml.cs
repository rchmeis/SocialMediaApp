using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp.Pages
{
    public class ImageUploadTestModel : PageModel
    {

        private readonly IImageService _imageService;

        public List<Image> Images { get; set; } = new List<Image>();

        [BindProperty]
        public IFormFile? UploadedImage { get; set; }

        [BindProperty]
        public string UserProvidedAltText { get; set; } = string.Empty;

        public ImageUploadTestModel(IImageService imageService)
        {
            _imageService = imageService;
        }

        public async Task OnGet(CancellationToken cancellationToken)
        {
            Images = await _imageService.GetAllAsync(cancellationToken);
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            string[] acceptedContentTypes = ["image/jpg", "image/jpeg", "image/png", "image/webp"];

            if (UploadedImage == null || UploadedImage.Length == 0)
            {
                ModelState.AddModelError(nameof(UploadedImage),"Vælg venligst et billede du vil uploade.");
                return Page();
            }

            if (!acceptedContentTypes.Contains(UploadedImage.ContentType))
            {
                ModelState.AddModelError(nameof(UploadedImage), "Billedet skal være i JPG, PNG eller WebP format.");
                return Page();
            }

            if (UploadedImage.Length > 64 * 1024 * 80) // Max 5 MB image size
            {
                ModelState.AddModelError(nameof(UploadedImage), "Billedet må højst være 5 MB.");
                return Page();
            }

            if (UserProvidedAltText.Length > 255)
            {
                ModelState.AddModelError(nameof(UserProvidedAltText), "Alternativ tekst må højst være 255 tegn.");
                return Page();
            }

            await _imageService.SaveImageAsync(UploadedImage, UserProvidedAltText, cancellationToken);

            return RedirectToPage("/ImageUploadTest");
        }
    }
}
