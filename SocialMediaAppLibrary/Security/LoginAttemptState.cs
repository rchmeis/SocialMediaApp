using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Security
{
    public sealed class LoginAttemptState //sealed classes cannot be inherited from
    {
        public int FailedAttempts { get; set; }
        public DateTimeOffset? LockedUntilUtc { get; set; }
    }
}
