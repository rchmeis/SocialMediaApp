using Microsoft.Extensions.Caching.Memory;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Security;
using SocialMediaAppLibrary.Services.Interfaces;
using System.Text.RegularExpressions;

namespace SocialMediaAppLibrary.Services
{
    public class VerificationService : IVerificationService
    {
        private const int MaxAttempts = 5;
        private const string AttemptsPrefix = "verify-attempts:";
        private const string ResendPrefix = "verify-resend:";

        private readonly IMemoryCache _cache;

        public VerificationService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public VerifyCheckResult CheckCode(string email, string? input, VerificationCode? code)
        {
            if (code == null || code.IsExpired)
            {
                return Failed("Koden er udløbet. Tryk på 'Send ny kode'.");
            }

            // attempts is 0 if nothing is in the cache yet
            _cache.TryGetValue(AttemptsPrefix + email, out int attempts);

            if (attempts >= MaxAttempts)
            {
                return Failed("Du har brugt for mange forsøg. Tryk på 'Send ny kode'.");
            }

            if (input == null || !Regex.IsMatch(input, @"^\d{6}$"))
            {
                return Failed("Koden skal være præcis 6 cifre.");
            }

            if (input != code.Code)
            {
                attempts++;
                _cache.Set(AttemptsPrefix + email, attempts, TimeSpan.FromMinutes(15));

                if (attempts >= MaxAttempts)
                {
                    return Failed("Du har brugt 5 forsøg. Koden er ugyldig. Tryk på 'Send ny kode'.");
                }

                return Failed("Koden er forkert. Du har " + (MaxAttempts - attempts) + " forsøg tilbage.");
            }

            // if code is correct, reset
            _cache.Remove(AttemptsPrefix + email);
            _cache.Remove(ResendPrefix + email);
            return new VerifyCheckResult { Succeeded = true };
        }

        // 0 = a new code may be sent now, otherwise the seconds left to wait
        public int GetSecondsUntilResend(string email)
        {
            if (_cache.TryGetValue(ResendPrefix + email, out DateTime availableAt))
            {
                double seconds = (availableAt - DateTime.UtcNow).TotalSeconds;
                if (seconds > 0)
                {
                    return (int)Math.Ceiling(seconds);
                }
            }
            return 0;
        }

        public void StartResendTimer(string email)
        {
            DateTime availableAt = DateTime.UtcNow.AddSeconds(60);
            _cache.Set(ResendPrefix + email, availableAt, TimeSpan.FromSeconds(60));
        }

        public void ResetAttempts(string email)
        {
            _cache.Remove(AttemptsPrefix + email);
        }

        private VerifyCheckResult Failed(string message)
        {
            return new VerifyCheckResult { Succeeded = false, ErrorMessage = message };
        }
    }
}