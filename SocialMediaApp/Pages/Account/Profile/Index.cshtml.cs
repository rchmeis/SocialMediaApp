using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SocialMediaApp.Security;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaApp.Pages.Account.Profile;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IStudentService _studentService;
    private readonly IPostService _postService;
    private readonly ILogger<IndexModel> _logger;

    private const string SidebarCookieName = "StudentHub.ProfileSidebarCollapsed";
    private const int PageSize = 10;

    public IndexModel(IStudentService studentService, IPostService postService, ILogger<IndexModel> logger)
    {
        _studentService = studentService;
        _postService = postService;
        _logger = logger;
    }

    public GenericPaginatedResult<Post> Posts { get; set; } = new GenericPaginatedResult<Post>();
    public Student CurrentStudent { get; set; } = new Student();
    public string PlaceholderText { get; set; } = "Hvad har du på hjerte?";
    public List<Post> PagedPosts { get; set; } = new List<Post>();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;

    public bool IsSidebarCollapsed { get; set; }

    
    public async Task<IActionResult> OnGetAsync(int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        IsSidebarCollapsed = ReadSidebarCookie();
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
            // LogError for unexpected errors (fx DB/repository)
            _logger.LogError(
                ex,
                "An error occurred while loading the profile page for page number {PageNumber}.",
                CurrentPage);

            IsSidebarCollapsed = ReadSidebarCookie();
            PlaceholderText = GenerateRandomPlaceholder();

            ModelState.AddModelError(string.Empty, "Der skete en fejl under hentning af siden. Kontakt systemadministrator, hvis problemet fortsætter.");

            return Page();
        }
    }

    
    public IActionResult OnPostToggleSidebar(int pageNumber = 1)
    {
        bool currentState = ReadSidebarCookie();
        bool nextState = !currentState;

        Response.Cookies.Append(
            SidebarCookieName,
            nextState ? "true" : "false",
            new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

        // Bliv på den samme side og det samme sidetal efter toggle
        return RedirectToPage(new { pageNumber = pageNumber < 1 ? 1 : pageNumber });
    }

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

    private bool ReadSidebarCookie()
    {
        if (Request.Cookies.TryGetValue(SidebarCookieName, out string? value))
        {
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        return false; // Standard: sidebaren er slået ud
    }

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