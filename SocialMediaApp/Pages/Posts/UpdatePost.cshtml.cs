using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaApp.Security;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;
using SocialMediaAppLibrary.Validation;


namespace SocialMediaApp.Pages.Posts
{
    /// <summary>
    /// Page model for editing an existing post. Lets the author change the title and content,
    /// add new images and delete existing ones. Only the post's author can access the page.
    /// </summary>
    public class UpdatePostModel : PageModel
    {
        private readonly IPostService _postService;
        private readonly IImageService _imageService;
        private readonly ILogger<UpdatePostModel> _logger;

        public UpdatePostModel(IPostService postService, ILogger<UpdatePostModel> logger, IImageService imageService)
        {
            _postService = postService;
            _logger = logger;
            _imageService = imageService;
        }
        
        /// <summary>
        /// The post being edited, loaded from the database. Used for display only and not bound from the form.
        /// </summary>
        public Post? PostToUpdate { get; set; }
        
        /// <summary>
        /// The local URL the user came from. After saving, the user is sent back here.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }
        
        /// <summary>
        /// The post's title, as entered in the form.
        /// </summary>
        [BindProperty]
        [MaxLength(63, ErrorMessage = "Titlen må højst være 63 tegn.")]
        public string? PostTitle { get; set; }

        /// <summary>
        /// The post's content, as entered in the form.
        /// </summary>
        [BindProperty]
        [Required(ErrorMessage = "Indholdet må ikke være tomt.")]
        [MaxLength(2000, ErrorMessage = "Indholdet må ikke overstige 2000 tegn.")]
        public string? PostContent { get; set; }

        /// <summary>
        /// New image files uploaded through the form.
        /// </summary>
        [BindProperty]
        [ListCount(0, 4, ErrorMessage = "Du kan højst vedhæfte 4 billeder pr. opslag.")]
        [AllowedContentTypes(["image/jpg", "image/jpeg", "image/png", "image/webp"], ErrorMessage = "Alle billeder skal være i JPG, PNG eller WebP format.")]
        [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Alle billeder må højst være 5 MB.")]
        public List<IFormFile> PostImages { get; set; } = new List<IFormFile>();
        
        
        /// <summary>
        /// Loads the post and pre-fills the form with its current title and content.
        /// </summary>
        /// <param name="postId">The unique identifier of the post to edit.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The page, or 404/403/a redirect if the post can't be loaded or the user isn't the author.</returns>
        public async Task<IActionResult> OnGetAsync(int postId, CancellationToken cancellationToken)
        {
            // calling helper method, to laod post
            IActionResult? result = await LoadPostAsync(postId, cancellationToken);
            if (result is not null)
            {
                return result;
            }
            
            // Prefilling the properties to update
            PostTitle = PostToUpdate!.Title;
            PostContent = PostToUpdate.Content;
            
            return Page();
        }

        /// <summary>
        /// Validates the form, optimizes any new images and saves the updated post.
        /// Number of images cannot be higher than 4.
        /// </summary>
        /// <param name="postId">The unique identifier of the post to update.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A redirect to <see cref="ReturnUrl"/> (or back to the page) with a success message,
        /// or the page with validation errors if the input is invalid or saving fails.
        /// </returns>
        public async Task<IActionResult> OnPostAsync(int postId, CancellationToken cancellationToken)
        {
            // calling helper method, to laod post
            IActionResult? result = await LoadPostAsync(postId, cancellationToken);
            
            if (result is not null)
            {
                return result;
            }
            
            // Existing images + new uploads must not exceed the limit
            if (PostToUpdate!.Images.Count + PostImages.Count > 4)
            {
                ModelState.AddModelError(nameof(PostImages), $"Du kan højst have 4 billeder pr. opslag.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            try
            {
                PostToUpdate.Title = PostTitle;
                PostToUpdate.Content = PostContent!;

                // Turn the uploaded files into Image objects (Id == 0 = new)
                foreach (IFormFile formFile in PostImages)
                {
                    Image optimizedImage = await _imageService.OptimizeImageAsync(formFile);
                    optimizedImage.AltText = optimizedImage.FileName;
                    PostToUpdate.Images.Add(optimizedImage);
                }
                
                await _postService.UpdateAsync(PostToUpdate.Id, PostToUpdate, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Couldn't update post {PostId}", postId);
                PostToUpdate.Images.RemoveAll(i => i.Id == 0);   // remove the unsaved images
                ModelState.AddModelError(string.Empty, "Noget gik galt ved at opdatere opslaget, prøv igen senere.");
                return Page();
            }
            
            TempData["Success"] = "Opslaget er opdateret";

            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return RedirectToPage(new { postId });
        }

        /// <summary>
        /// Deletes a single image from the post. The image must belong to the post being edited.
        /// </summary>
        /// <param name="postId">The unique identifier of the post the image belongs to.</param>
        /// <param name="imageId">The unique identifier of the image to delete.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A redirect back to the page, or 400 if the image doesn't belong to the post.</returns>
        public async Task<IActionResult> OnPostDeleteAsync(int postId, int imageId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting image {ImageId} from post {PostId}", imageId, postId);

            
            IActionResult? result = await LoadPostAsync(postId, cancellationToken);
            if (result is not null)
            {
                return result;
            }

            if (!PostToUpdate!.Images.Any(i => i.Id == imageId))
            {
                return BadRequest();
            }

            await _imageService.DeleteAsync(imageId, cancellationToken);
            return RedirectToPage(new { postId, returnUrl = ReturnUrl });
        }
        
        /// <summary>
        /// Loads the post into <see cref="PostToUpdate"/> and checks that the logged-in user is its author.
        /// </summary>
        /// <param name="postId">The unique identifier of the post to load.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>null</c> if the post was loaded and the user may edit it; otherwise a result that stops the handler:
        /// a redirect to the feed on errors, 404 if the post doesn't exist, or 403 if the user isn't the author.
        /// </returns>
        private async Task<IActionResult?> LoadPostAsync(int postId, CancellationToken cancellationToken)
        {
            try
            {
                PostToUpdate = await _postService.ReadByIdAsync(postId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Couldn't load post {PostId}", postId);
                TempData["Error"] = "Noget gik galt ved at loade post, prøv igen senere";
                return RedirectToPage("/Feed");
            }

            if (PostToUpdate is null)
            {
                return NotFound();
            }

            if (PostToUpdate.Author.Id != ClaimsPrincipalExtensions.GetUserId(User))
            {
                
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            return null;
        }
    }
}
