using SocialMediaAppLibrary.Models;

namespace SocialMediaAppLibrary.Services.Interfaces;

public interface IVerificationService
{
    VerifyCheckResult CheckCode(string email, string? input, VerificationCode? code);
    int GetSecondsUntilResend(string email);
    void StartResendTimer(string email);
    void ResetAttempts(string email);
}