using Microsoft.Extensions.Caching.Memory;
using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Security
{
    public sealed class LoginCheckResult //sealed classes cannot be inherited from
    {
        public bool Succeeded { get; init; }
        public UserAuthentication? UserAuthentication { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
