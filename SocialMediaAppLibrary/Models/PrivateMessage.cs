using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class PrivateMessage
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; }
        public User Sender { get; set; }
        public User Receiver { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ReadAt { get; set; }

    }
}
