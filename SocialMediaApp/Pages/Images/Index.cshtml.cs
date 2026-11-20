using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp.Pages.Images
{
    public class IndexModel : PageModel
    {
        private readonly IImageService _imageService;

        public IndexModel(IImageService imageService)
        {
            _imageService = imageService;
        }

        public async Task<IActionResult> OnGet(int id, string filename)
        {
            Image? image = await _imageService.GetByIdAsync(id);

            if (image == null || image.FileName != filename)
            {
                return NotFound();
            }

            Response.Headers.CacheControl = "private, max-age=86400"; // Requires authentication and cache image for 1 day (86400 seconds)

            return File(image.ImageData, "image/webp");
        }
    }
}
