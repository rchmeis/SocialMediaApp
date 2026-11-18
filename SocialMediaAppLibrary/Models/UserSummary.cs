using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class UserSummary
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }        
        public Image? image { get; set; }
        public bool IsPartner { get;set; } = false; //IF we want a partner badge OR a separate profile URL
    }
    }
}
