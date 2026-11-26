using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaApp.Security;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp.Pages.Account.Profile;

/// <summary>
/// PageModel for the authenticated user's profile page, handling profile loading and paginated post feed retrieval.
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly IStudentService _studentService;
    private readonly IPostService _postService;
    private readonly ILogger<IndexModel> _logger;

    /// <summary>
    /// Number of posts displayed per page on the profile feed.
    /// </summary>
    private const int PageSize = 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexModel"/> class.
    /// </summary>
    /// <param name="studentService">Service used to retrieve student profile details.</param>
    /// <param name="postService">Service used to retrieve paginated posts for the user.</param>
    /// <param name="logger">Logger used to record warnings and errors during page execution.</param>
    public IndexModel(IStudentService studentService, IPostService postService, ILogger<IndexModel> logger)
    {
        _studentService = studentService;
        _postService = postService;
        _logger = logger;
    }

    /// <summary>
    /// Gets or sets the full paginated result container returned by the post service.
    /// </summary>
    public GenericPaginatedResult<Post> Posts { get; set; } = new GenericPaginatedResult<Post>();

    /// <summary>
    /// Gets or sets the student profile associated with the currently signed-in user.
    /// </summary>
    public Student CurrentStudent { get; set; } = new Student();

    /// <summary>
    /// Gets or sets the placeholder prompt displayed in the new-post input control.
    /// </summary>
    public string PlaceholderText { get; set; } = "Hvad har du på hjerte?";

    /// <summary>
    /// Gets or sets the posts belonging to the requested page.
    /// </summary>
    public List<Post> PagedPosts { get; set; } = new List<Post>();

    /// <summary>
    /// Gets or sets the 1-based index of the currently active page (minimum value is 1).
    /// </summary>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// Gets or sets the total number of pages available for the user's posts (minimum value is 1).
    /// </summary>
    public int TotalPages { get; set; } = 1;

    /// <summary>
    /// Handles HTTP GET requests for the profile page, validates the current user and profile,
    /// and loads the requested page of posts.
    /// </summary>
    /// <param name="pageNumber">The 1-based page number requested via query string. Defaults to <c>1</c>.</param>
    /// <param name="cancellationToken">Token used to cancel asynchronous operations if the request aborts.</param>
    /// <returns>
    /// The rendered <see cref="PageModel"/> on success or recoverable error, or a redirect to the sign-in page
    /// if no student profile is linked or user claim extraction fails.
    /// </returns>
    public async Task<IActionResult> OnGetAsync(int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        PlaceholderText = GenerateRandomPlaceholder();

        // make sure current page is always one
        CurrentPage = pageNumber < 1 ? 1 : pageNumber;

        try
        {
            int userId = ClaimsPrincipalExtensions.GetUserId(User);
            var student = await _studentService.ReadByIdAsync(userId, cancellationToken);
            if (student == null)
            {
                TempData["Error"] = "Hov - der er opstået en fejl. Der er ikke tilknyttet en profil til din bruger";
                return RedirectToPage("/Account/SignIn");
            }

            CurrentStudent = student;

            // Hent 10 posts pr. side for den aktuelle bruger
            await LoadUserPostsAsync(CurrentStudent.Id, CurrentPage, cancellationToken);
            return Page();
        }
        catch (UnauthorizedAccessException ex)
        {
            var nameIdentifierValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogWarning(
                ex,
                "Failed to extract valid {ClaimType} from current user. Raw value: {ClaimValue}",
                ClaimTypes.NameIdentifier,
                nameIdentifierValue ?? "<null>");

            await HttpContext.SignOutAsync();
            TempData["Error"] = "Hov - der er opstået en fejl i dit brugerlogin. Log ind igen og kontakt systemadministrator hvis problemet vedbliver.";

            return RedirectToPage("/Account/SignIn");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while loading the profile page for page number {PageNumber}.",
                CurrentPage);

            PlaceholderText = GenerateRandomPlaceholder();

            ModelState.AddModelError(string.Empty, "Der skete en fejl under hentning af siden. Kontakt systemadministrator, hvis problemet fortsætter.");

            return Page();
        }
    }

    /// <summary>
    /// Handles HTTP POST requests to soft-delete a post and redirects back to the current feed page.
    /// </summary>
    /// <param name="postId">The identifier of the post to delete.</param>
    /// <param name="pageNumber">The 1-based page number to return to after deletion.</param>
    /// <param name="cancellationToken">Token used to cancel asynchronous operations if the request aborts.</param>
    /// <returns>A redirect to the profile page for the requested page number.</returns>
    public async Task<IActionResult> OnPostDeletePostAsync(int postId, int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        int safePageNumber = pageNumber < 1 ? 1 : pageNumber;

        try
        {
            int userId = ClaimsPrincipalExtensions.GetUserId(User);
            var student = await _studentService.ReadByIdAsync(userId, cancellationToken);
            if (student == null)
            {
                TempData["Error"] = "Hov - der er opstået en fejl. Der er ikke tilknyttet en profil til din bruger";
                return RedirectToPage("/Account/SignIn");
            }

            await _postService.DeleteAsync(postId, cancellationToken);

            //TAKING CARE OF EDGE CASES (deleting the last post on page 2, makes sure user gets redirected to page 1
            var options = new PaginationOptions
            {
                PageNumber = safePageNumber,
                PageSize = PageSize
            };

            GenericPaginatedResult<Post> feed = await _postService.GetFeedByIdAsync(student.Id, options, cancellationToken);
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)feed.TotalCount / PageSize));
            int targetPage = Math.Min(safePageNumber, totalPages);

            return RedirectToPage(new { pageNumber = targetPage });
        }
        catch (UnauthorizedAccessException ex)
        {
            var nameIdentifierValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogWarning(
                ex,
                "Failed to extract valid {ClaimType} from current user. Raw value: {ClaimValue}",
                ClaimTypes.NameIdentifier,
                nameIdentifierValue ?? "<null>");

            await HttpContext.SignOutAsync();
            TempData["Error"] = "Hov - der er opstået en fejl i dit brugerlogin. Log ind igen og kontakt systemadministrator hvis problemet vedbliver.";

            return RedirectToPage("/Account/SignIn");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while deleting post {PostId} on page {PageNumber}.",
                postId,
                safePageNumber);

            TempData["Error"] = "Der skete en fejl under sletning af opslaget.";
            return RedirectToPage(new { pageNumber = safePageNumber });
        }
    }

    /// <summary>
    /// Fetches a single page of posts for the specified student and updates <see cref="PagedPosts"/> and <see cref="TotalPages"/>.
    /// </summary>
    /// <param name="studentId">The identifier of the student whose posts are being loaded.</param>
    /// <param name="pageNumber">The 1-based page number to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel asynchronous operations if the request aborts.</param>
    private async Task LoadUserPostsAsync(int studentId, int pageNumber, CancellationToken cancellationToken)
    {
        var options = new PaginationOptions
        {
            PageNumber = pageNumber,
            PageSize = PageSize
        };

        GenericPaginatedResult<Post> result = await _postService.GetFeedByIdAsync(studentId, options, cancellationToken);
       
        PagedPosts = result.Items;
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)result.TotalCount / PageSize));
    }


    /// <summary>
    /// Selects a random Danish placeholder prompt for the post composer.
    /// </summary>
    /// <returns>A randomly chosen placeholder string.</returns>
    private string GenerateRandomPlaceholder()
    {
        List<string> placeholders = new List<string>
        {
            "Hvad har du på hjerte?",
            "Hvad er dit næste projekt?",
            "Hvad inspirerer dig i dag?",
            "Fortæl os om din dag!",
            "Hvilke oplevelser vil du dele?"
        };

        return placeholders[Random.Shared.Next(placeholders.Count)];
    }
}