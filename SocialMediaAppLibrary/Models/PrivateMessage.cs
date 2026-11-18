using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class PrivateMessage
    {
        public int Id { get; set; }        
        public string Content { get; set; }
        public UserSummary Sender { get; set; }
        public UserSummary Receiver { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? DeletedAt { get; set; }

    }
}
