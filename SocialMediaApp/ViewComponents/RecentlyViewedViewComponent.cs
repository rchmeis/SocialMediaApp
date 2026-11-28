using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Services.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace SocialMediaApp.ViewComponents
{
    public class RecentlyViewedViewComponent : ViewComponent
    {
        private const int MaxRecentProfiles = 5;
        private readonly IUserService _userService;

        public RecentlyViewedViewComponent(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
        {
            if (HttpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return View(new List<UserSummary>());
            }

            string? userIdClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int loggedInUserId))
            {
                return View(new List<UserSummary>());
            }

            string cookieName = $"recent_profiles_{loggedInUserId}";
            if (!HttpContext.Request.Cookies.TryGetValue(cookieName, out string? existingJson) ||
                string.IsNullOrWhiteSpace(existingJson))
            {
                return View(new List<UserSummary>());
            }

            List<int> recentIds;
            try
            {
                recentIds = JsonSerializer.Deserialize<List<int>>(existingJson) ?? new List<int>();
            }
            catch (JsonException)
            {
                return View(new List<UserSummary>());
            }

            recentIds = recentIds
                .Distinct()
                .Take(MaxRecentProfiles)
                .ToList();

            if (recentIds.Count == 0)
            {
                return View(new List<UserSummary>());
            }

            // Adjust method name to match your IUserService (e.g., GetSummariesByIdsAsync)
            List<UserSummary> summaries = await _userService.GetSummariesByIdsAsync(recentIds, cancellationToken);
            Dictionary<int, UserSummary> byId = summaries.ToDictionary(u => u.Id);

            List<UserSummary> orderedSummaries = recentIds //looks up UserSummaries using the ordered list from the cookie and a dictionary for key-based-lookup
                .Where(id => byId.ContainsKey(id))   //filters out any ID from the cookie that wasn't returned in summaries (for example, if that user account was deleted).
                .Select(id => byId[id])
                .ToList();

            return View(orderedSummaries);
        }

        //NOTE____________
        //When InvokeAsync calls return View(orderedSummaries); without specifying a view name, ASP.NET Core defaults the view name to Default and searches these convention paths in order:

        ///Pages/Components/RecentlyViewed/Default.cshtml
        ///Pages/Shared/Components/RecentlyViewed/Default.cshtml
        ///Views/Shared/Components/RecentlyViewed/Default.cshtml
    }
}