using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Security
{
    public sealed class LoginAttemptState //sealed classes cannot be inherited from
    {
        public int FailedAttempts { get; set; } = 0;
        public DateTimeOffset? LockedUntilUtc { get; set; }
    }
}
