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
            string cacheKey = $"image_{id}";

            // Try to get the image from the cache,
            // if not found, retrieve it from the service and cache it to save the database from heavy load
            Image? image = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1); // Cache in memory for 1 day

                return await _imageService.GetByIdAsync(id, cancellationToken);
            });

            if (image == null || image.FileName != filename)
            {
                return NotFound();
            }

            // Generate an ETag based on the image data to help with caching and validation
            string etag = $"\"{Convert.ToHexString(SHA256.HashData(image.ImageData))}\"";

            Response.Headers.ETag = etag;
            Response.Headers.CacheControl = "private, max-age=86400"; // Requires authentication and cache image for 1 day (86400 seconds) on the user's browser

            // Check whether the browser already has the current image, if so, return a 304 Not Modified status to avoid sending the image data again
            if (Request.Headers.IfNoneMatch.Any(x => x == etag || x == "*"))
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            return File(image.ImageData, "image/webp");
        }
    }
}
