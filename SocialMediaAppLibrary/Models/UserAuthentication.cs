using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class UserAuthentication
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHashed { get; set; }
        public bool IsVerified { get; set; } = false;
        public Role Role { get; set; }
    }
}
