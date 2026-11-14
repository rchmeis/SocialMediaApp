using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class UserAuthDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }
        public int UserId { get; set; }
    }
}
