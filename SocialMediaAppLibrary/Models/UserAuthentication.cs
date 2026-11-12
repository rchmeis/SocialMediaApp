using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class UserAuthentication
    {
        public string Username { get; set; }
        public string PasswordHashed { get; set; }
    }
}
