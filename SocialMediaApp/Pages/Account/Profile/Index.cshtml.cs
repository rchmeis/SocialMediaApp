using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaApp.Security;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp.Pages.Account.Profile;

/// <summary>
/// PageModel for the profile page, supporting both the authenticated user's own profile and other users' profiles,
/// follow/unfollow toggling, post deletion, and paginated post feed retrieval.
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly IStudentService _studentService;
    private readonly IPostService _postService;
    private readonly ILogger<IndexModel> _logger;
    private readonly IUserService _userService;

    /// <summary>
    /// Number of posts displayed per page on the profile feed.
    /// </summary>
    private const int PageSize = 10;

    /// <summary>
    /// Maximum number of profiles stored in the users brower through cookies.
    /// </summary>
    private const int MaxRecentProfiles = 5;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexModel"/> class.
    /// </summary>
    /// <param name="studentService">Service used to retrieve student profile details.</param>
    /// <param name="postService">Service used to retrieve paginated posts for the user.</param>
    /// <param name="logger">Logger used to record warnings and errors during page execution.</param>
    /// <param name="userservice">Service used to retrieve follow counts and manage follow relationships.</param>
    public IndexModel(IStudentService studentService, IPostService postService, ILogger<IndexModel> logger, IUserService userservice)
    {
        _studentService = studentService;
        _postService = postService;
        _logger = logger;
        _userService = userservice;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the viewed profile belongs to the currently signed-in user.
    /// </summary>
    public bool IsOwnProfile { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the profile currently being viewed.
    /// </summary>
    public int ProfileUserId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the signed-in user currently follows the viewed profile.
    /// </summary>
    public bool IsFollowing { get; set; }

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
    /// Gets or set the total number of followers for the current user
    /// </summary>
    public int Followers { get; set; } = 0;

    /// <summary>
    /// Gets or sets the total number of users the current user is following
    /// </summary>
    public int Following { get; set; } = 0;

    /// <summary>
    /// Handles HTTP GET requests for the profile page, loads the target user's profile and follow state,
    /// and retrieves the requested page of posts.
    /// </summary>
    /// <param name="id">Optional target user ID from the query string; defaults to the signed-in user when omitted.</param>
    /// <param name="pageNumber">The 1-based page number requested via query string. Defaults to <c>1</c>.</param>
    /// <param name="cancellationToken">Token used to cancel asynchronous operations if the request aborts.</param>
    /// <returns>
    /// The rendered <see cref="PageModel"/> on success or recoverable error, or a redirect when profile loading fails
    /// or user claim extraction fails.
    /// </returns>
    public async Task<IActionResult> OnGetAsync(int?id, int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        PlaceholderText = GenerateRandomPlaceholder();
        CurrentPage = pageNumber < 1 ? 1 : pageNumber;

        try
        {
            int loggedInUserId = ClaimsPrincipalExtensions.GetUserId(User);
            int targetUserId = id ?? loggedInUserId;
            IsOwnProfile = targetUserId == loggedInUserId;           
            ProfileUserId = targetUserId;

            var student = await _studentService.ReadByIdAsync(targetUserId, cancellationToken);
            if (student == null)
            {
                if (IsOwnProfile)
                {
                    TempData["Error"] = "Hov - der er opstået en fejl. Der er ikke tilknyttet en profil til din bruger";
                    return RedirectToPage("/Account/SignIn");
                }
                TempData["Error"] = "Profilen blev ikke fundet.";
                return RedirectToPage("/Feed");
            }
            if (!IsOwnProfile)
            {
                SaveRecentlyViewedProfile(loggedInUserId, targetUserId);
            }

            CurrentStudent = student;
            Following = await _userService.GetFollowingCountAsync(targetUserId, cancellationToken);
            Followers = await _userService.GetFollowersCountAsync(targetUserId, cancellationToken);

            if (!IsOwnProfile)
            {
                IsFollowing = await _userService.IsFollowingAsync(loggedInUserId, targetUserId, cancellationToken);
            }

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
                "An error occurred while loading the profile page for user {TargetUserId} and page {PageNumber}.",
                ProfileUserId, CurrentPage);

            PlaceholderText = GenerateRandomPlaceholder();

            ModelState.AddModelError(string.Empty, "Der skete en fejl under hentning af siden. Kontakt systemadministrator, hvis problemet fortsætter.");

            return Page();
        }
    }

    /// <summary>
    /// Handles HTTP POST requests to toggle whether the signed-in user follows the specified target user.
    /// </summary>
    /// <param name="id">The identifier of the target user to follow or unfollow.</param>
    /// <param name="pageNumber">The 1-based feed page number to preserve on redirect. Defaults to <c>1</c>.</param>
    /// <param name="cancellationToken">Token used to cancel asynchronous operations if the request aborts.</param>
    /// <returns>A redirect back to the target user's profile page.</returns>
    public async Task<IActionResult> OnPostToggleFollowAsync(int id, int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        int safePageNumber = pageNumber < 1 ? 1 : pageNumber;

        try
        {
            int loggedInUserId = ClaimsPrincipalExtensions.GetUserId(User);

            if (loggedInUserId == id) //ekstra beskyttelse. sammenligner id for den bruger der er logget ind med den bruger man forsøger at følge
            {
                return RedirectToPage(new { id, pageNumber = safePageNumber });
            }

            bool alreadyFollowing = await _userService.IsFollowingAsync(loggedInUserId, id, cancellationToken);

            if (alreadyFollowing)
            {
                await _userService.UnfollowAsync(loggedInUserId, id, cancellationToken);
            }
            else
            {
                await _userService.FollowAsync(loggedInUserId, id, cancellationToken);
            }

            return RedirectToPage(new { id, pageNumber = safePageNumber });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to toggle follow for target user {TargetUserId}.", id);
            await HttpContext.SignOutAsync();
            TempData["Error"] = "Hov - der er opstået en fejl i dit brugerlogin. Log ind igen og kontakt systemadministrator hvis problemet vedbliver.";
            return RedirectToPage("/Account/SignIn");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling follow status for target user {TargetUserId}.", id);
            TempData["Error"] = "Der skete en fejl under opdatering af følger-status.";
            return RedirectToPage(new { id, pageNumber = safePageNumber });
        }
    }


    /// <summary>
    /// Handles HTTP POST requests to soft-delete a post and redirects back to the current feed page.
    /// </summary>
    /// <param name="postId">The identifier of the post to delete.</param>
    /// <param name="pageNumber">The 1-based page number to return to after deletion.</param>
    /// <param name="cancellationToken">Token used to cancel asynchronous operations if the request aborts.</param>
    /// <returns>A redirect to the profile page for the requested page number.</returns>
    public async Task<IActionResult> OnPostDeletePostAsync(int postId, string? returnUrl = null, int pageNumber = 1, CancellationToken cancellationToken = default)
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

            //TAKING CARE OF EDGE CASES (eg deleting the last post on page 2, makes sure user gets redirected to page 1
            var options = new PaginationOptions
            {
                PageNumber = safePageNumber,
                PageSize = PageSize
            };

            GenericPaginatedResult<Post> feed = await _postService.GetFeedByIdAsync(student.Id, options, cancellationToken);
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)feed.TotalCount / PageSize));
            int targetPage = Math.Min(safePageNumber, totalPages);

            TempData["Success"] = "Opslaget er slettet";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
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

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
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
    /// Records a recently viewed profile identifier in a per-user cookie ordered newest-first,
    /// deduplicating repeat visits and capping the stored list at the maximum allowed count.
    /// </summary>
    /// <param name="loggedInUserId">The identifier of the currently signed-in user whose cookie is updated.</param>
    /// <param name="viewedProfileId">The identifier of the profile being viewed.</param>
    private void SaveRecentlyViewedProfile(int loggedInUserId, int viewedProfileId)
    {
        string cookieName = $"recent_profiles_{loggedInUserId}";
        List<int> recentIds = new();

        if (Request.Cookies.TryGetValue(cookieName, out string? existingJson) &&
            !string.IsNullOrWhiteSpace(existingJson))
        {
            try
            {
                recentIds = JsonSerializer.Deserialize<List<int>>(existingJson) ?? new List<int>();
            }
            catch (JsonException)
            {
                recentIds = new List<int>();
            }
        }

        recentIds.Remove(viewedProfileId);
        recentIds.Insert(0, viewedProfileId);

        if (recentIds.Count > MaxRecentProfiles)
        {
            recentIds.RemoveRange(MaxRecentProfiles, recentIds.Count - MaxRecentProfiles); //starts at given index, removes given amount after the comma
        }

        Response.Cookies.Append(cookieName, JsonSerializer.Serialize(recentIds), new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps
        });
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