using System.Security.Claims;
namespace SocialMediaApp.Security
{
    /// <summary>
    /// Provides extension methods for <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Retrieves the currently authenticated user's ID from the <see cref="ClaimTypes.NameIdentifier"/> claim.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> to read the user ID from.</param>
        /// <returns>The user's ID as an integer.</returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown when a valid integer user ID is missing from the user's claims.
        /// </exception>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var rawId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(rawId, out var userId))
            {
                throw new UnauthorizedAccessException("Gyldigt bruger-id mangler i claims.");
            }

            return userId;
        }
    }
    
}
