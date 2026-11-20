using Mysqlx.Crud;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SocialMediaAppLibrary.Validation
{
    /// <summary>
    /// Provides shared validation constants, regular expressions, and helper methods
    /// for account registration fields such as usernames, passwords, and student email domains.
    /// </summary>
    public static partial class AccountValidationRules
    {
        /// <summary>
        /// The minimum allowed character length for a username.
        /// </summary>        
        public const int UsernameMinLength = 3;
        /// <summary>
        /// The maximum allowed character length for a username.
        /// </summary>
        public const int UsernameMaxLength = 20;
        /// <summary>
        /// The regular expression pattern defining valid usernames (3–20 letters, digits, dots, or underscores).
        /// </summary>
        public const string UsernamePattern = @"^[a-zA-Z0-9._]{3,20}$";
        /// <summary>
        /// The localized error message shown when a username fails validation.
        /// </summary>
        public const string UsernameErrorMessage =
            "Brugernavnet skal være 3–20 tegn og må kun indeholde bogstaver, tal, punktum og understreg.";


        /// <summary>
        /// The minimum required character length for a password.
        /// </summary>
        public const int PasswordMinLength = 8;
        /// <summary>
        /// The maximum allowed character length for a password.
        /// </summary>
        public const int PasswordMaxLength = 128;
        /// <summary>
        /// The regular expression pattern requiring at least one lowercase letter,
        /// one uppercase letter, one digit, and at least 8 characters.
        /// </summary>
        public const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"; //positive lookahead(?=...) starts at beginning and scans through the string (.*) to see if there is at least one of the characters in the range []. Once found, it jumps back without having "consumed" any characters.
        /// <summary>
        /// The localized error message shown when a password fails validation.
        /// </summary>
        public const string PasswordErrorMessage =
            "Adgangskoden skal være mindst 8 tegn og indeholde mindst ét stort bogstav, ét lille bogstav og ét tal.";



        /// <summary>
        /// The localized error message shown when a student email address does not belong
        /// to an approved educational institution domain.
        /// </summary>
        public const string StudentEmailDomainErrorMessage =
       "Studiemailen skal tilhøre en godkendt uddannelsesinstitution.";



        /// <summary>
        /// The maximum allowed character length for an email address.
        /// </summary>
        public const int EmailMaxLength = 254;
        /// <summary>
        /// The localized error message shown when an email address format is invalid.
        /// </summary>
        public const string EmailInvalidMessage = "Indtast en gyldig e-mailadresse.";



        /// <summary>
        /// Gets a source-generated <see cref="Regex"/> instance for validating usernames
        /// with a 100 ms match timeout.
        /// </summary>
        /// <returns>A compiled/generated <see cref="Regex"/> matching <see cref="UsernamePattern"/>.</returns>
        [GeneratedRegex(UsernamePattern, RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
        public static partial Regex UsernameRegex();

        /// <summary>
        /// Gets a source-generated <see cref="Regex"/> instance for validating passwords
        /// with a 100 ms match timeout.
        /// </summary>
        /// <returns>A compiled/generated <see cref="Regex"/> matching <see cref="PasswordPattern"/>.</returns>
        [GeneratedRegex(PasswordPattern, RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
        public static partial Regex PasswordRegex();



        /// <summary>
        /// Determines whether the specified username is non-empty and matches <see cref="UsernamePattern"/>.
        /// </summary>
        /// <param name="username">The username candidate to validate.</param>
        /// <returns>
        /// <see langword="true"/> if the username is valid; otherwise, <see langword="false"/>
        /// (including if a regex match timeout occurs).
        /// </param>
        public static bool IsValidUsername(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            try
            {
                return UsernameRegex().IsMatch(username);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified password meets length limits and matches <see cref="PasswordPattern"/>.
        /// </summary>
        /// <param name="password">The password candidate to validate.</param>
        /// <returns>
        /// <see langword="true"/> if the password is valid and within <see cref="PasswordMaxLength"/>;
        /// otherwise, <see langword="false"/> (including if a regex match timeout occurs).
        /// </param>
        public static bool IsValidPassword(string? password)
        {
            if (string.IsNullOrEmpty(password) || password.Length > PasswordMaxLength)
                return false;

            try
            {
                return PasswordRegex().IsMatch(password);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

       

        /// <summary>
        /// Checks whether an email address belongs to—or is a subdomain of—any allowed institution domain.
        /// </summary>
        /// <param name="email">The email address to inspect.</param>
        /// <param name="allowedDomains">The collection of approved institution domains (e.g., <c>"via.dk"</c>).</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="email"/> has a domain matching or ending with
        /// <c>.&lt;allowedDomain&gt;</c>; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool HasAllowedStudentDomain(string? email, IEnumerable<string> allowedDomains)
        {
            if (string.IsNullOrWhiteSpace(email) || allowedDomains is null)
                return false;

            string trimmedEmail = email.Trim();
            int atIndex = trimmedEmail.LastIndexOf('@'); //Finds the position of '@'
            if (atIndex < 0 || atIndex == trimmedEmail.Length - 1)
            {
                return false;
            }
                

            string domain = trimmedEmail[(atIndex + 1)..]; //Grabs everything AFTER '@' using range operator [5..]

            return allowedDomains
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .Select(d => d.Trim().TrimStart('@').ToLowerInvariant())
                    .Any(allowed =>
                        string.Equals(domain, allowed, StringComparison.OrdinalIgnoreCase) || //checks for an exact match
                        domain.EndsWith($".{allowed}", StringComparison.OrdinalIgnoreCase));  //Checks if the user's domain is a subdomain of that institution
        }
    }
}
