using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp.Pages.Images
{
    public class IndexModel : PageModel
    {
        private readonly IImageService _imageService;
        private readonly IMemoryCache _cache;

        public IndexModel(IImageService imageService, IMemoryCache cache)
        {
            _imageService = imageService;
            _cache = cache;
        }

        public async Task<IActionResult> OnGetAsync(int id, string filename, CancellationToken cancellationToken)
        {
            Image? image = await _imageService.GetByIdAsync(id, cancellationToken);

            if (image == null || image.FileName != filename)
            {
                return NotFound();
            }

            // Requires authentication and cache the image for 1 day (86400 seconds) on the user's browser
            Response.Headers.CacheControl = "private, max-age=86400, immutable";

            return File(image.ImageData, "image/webp");
        }
    }
}
