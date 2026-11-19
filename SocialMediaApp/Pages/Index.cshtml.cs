using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IImageService _imageService;
        public IndexModel(IImageService imageService)
        {
            _imageService = imageService;
        }

        public void OnGet()
        {

        }

        
        /// <summary>
        /// Handles the HTTP GET request to retrieve and serve a user's image as a binary file stream from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the image to retrieve.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the image file (JPEG) if found; 
        /// otherwise, a <see cref="NotFoundResult"/> if the image record or its byte data is missing.
        /// </returns>
        public async Task<IActionResult> OnGetUsertypeImageAsync(int id)
        {
            var imageRecord = await _imageService.GetByIdAsync(id);

            if (imageRecord == null || imageRecord.ImageData == null)
            {
                return NotFound();
            }

            // 2. we take the raw byte[] from the database and deliver them as a real image
            return File(imageRecord.ImageData, "image/jpeg");
        }
        // The method runs when the HTML tag requests it through, for examaple _Layout <img src="/Index?handler=UserImage&id=5"/>:
    }
}
