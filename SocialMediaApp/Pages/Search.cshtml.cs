using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace SocialMediaApp.Pages
{
    /// <summary>
    /// Handles profile search queries and actions for managing recently viewed profile cookies.
    /// </summary>
    public class SearchModel : PageModel
    {
        /// <summary>
        /// Handles HTTP GET requests for the search page.
        /// </summary>
        private const int MaxRecentProfiles = 5;

        public void OnGet()
        {
        }

        /// <summary>
        /// Removes a specific profile identifier from the signed-in user's recently viewed cookie
        /// and redirects back to the originating local page.
        /// </summary>
        /// <param name="profileId">The identifier of the profile to remove from the recent list.</param>
        /// <param name="returnUrl">An optional local URL to redirect to after updating the cookie.</param>
        /// <returns>A local redirect to <paramref name="returnUrl"/> if valid; otherwise, a redirect to <c>/Index</c>.</returns>
        public IActionResult OnPostRemoveRecent(int profileId, string? returnUrl = null)
        {
            if (TryGetLoggedInUserId(out int loggedInUserId))
            {
                string cookieName = $"recent_profiles_{loggedInUserId}";
                List<int> recentIds = ReadRecentProfileIds(cookieName);

                recentIds.RemoveAll(id => id == profileId);
                recentIds = recentIds.Distinct().Take(MaxRecentProfiles).ToList();

                if (recentIds.Count == 0)
                {
                    Response.Cookies.Delete(cookieName);
                }
                else
                {
                    WriteRecentProfileIds(cookieName, recentIds);
                }
            }

            return RedirectToSafeLocalUrl(returnUrl);
        }

        /// <summary>
        /// Deletes the recently viewed profile cookie for the signed-in user
        /// and redirects back to the originating local page.
        /// </summary>
        /// <param name="returnUrl">An optional local URL to redirect to after clearing the cookie.</param>
        /// <returns>A local redirect to <paramref name="returnUrl"/> if valid; otherwise, a redirect to <c>/Index</c>.</returns>
        public IActionResult OnPostClearRecent(string? returnUrl = null)
        {
            if (TryGetLoggedInUserId(out int loggedInUserId))
            {
                string cookieName = $"recent_profiles_{loggedInUserId}";
                Response.Cookies.Delete(cookieName);
            }

            return RedirectToSafeLocalUrl(returnUrl);
        }

        /// <summary>
        /// Attempts to resolve a positive integer user identifier from the current user's claims.
        /// </summary>
        /// <param name="userId">When this method returns <see langword="true"/>, contains the authenticated user's identifier; otherwise, <c>0</c>.</param>
        /// <returns><see langword="true"/> if the user is authenticated and has a valid integer <see cref="ClaimTypes.NameIdentifier"/> claim; otherwise, <see langword="false"/>.</returns>
        private bool TryGetLoggedInUserId(out int userId)
        {
            userId = 0;

            if (User?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out userId) && userId > 0;
        }

        /// <summary>
        /// Reads and deserializes a list of recently viewed profile identifiers from a cookie.
        /// </summary>
        /// <param name="cookieName">The name of the cookie containing serialized profile identifiers.</param>
        /// <returns>The deserialized list of profile identifiers, or an empty list if missing or invalid JSON.</returns>
        private List<int> ReadRecentProfileIds(string cookieName)
        {
            if (!Request.Cookies.TryGetValue(cookieName, out string? existingJson) ||
                string.IsNullOrWhiteSpace(existingJson))
            {
                return new List<int>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<int>>(existingJson) ?? new List<int>();
            }
            catch (JsonException)
            {
                return new List<int>();
            }
        }

        /// <summary>
        /// Serializes and writes the list of recently viewed profile identifiers to a cookie.
        /// </summary>
        /// <param name="cookieName">The name of the target cookie.</param>
        /// <param name="ids">The ordered list of profile identifiers to persist.</param>
        private void WriteRecentProfileIds(string cookieName, List<int> ids)
        {
            string json = JsonSerializer.Serialize(ids);

            CookieOptions options = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            };

            Response.Cookies.Append(cookieName, json, options);
        }

        /// <summary>
        /// Redirects to <paramref name="returnUrl"/> when it is a safe local URL; otherwise, redirects to <c>/Index</c>.
        /// </summary>
        /// <param name="returnUrl">The candidate return URL to validate.</param>
        /// <returns>A local redirect result to <paramref name="returnUrl"/> or a page redirect to <c>/Index</c>.</returns>
        private IActionResult RedirectToSafeLocalUrl(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToPage("/Index");
        }
    }
}