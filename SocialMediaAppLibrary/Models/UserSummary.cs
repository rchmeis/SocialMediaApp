using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class UserSummary
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }        
        public Image? ProfileImage { get; set; }    
        public bool isOnboarded { get; set; }
    }
    
}
