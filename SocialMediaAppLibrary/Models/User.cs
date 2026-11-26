using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class User
    {
        public int Id { get; set; }
        public int UserAuthenticationId { get; set; }
        public Image? Image { get; set; }
        public string Bio { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
