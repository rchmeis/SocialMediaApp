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
        //public List<PrivateMessage> SentMessages { get; set; } = new List<PrivateMessage>(); bør fjernes. MessageViewmodel i Inbox henter dem ud fra loggede in users ID
        //public List<PrivateMessage> ReceivedMessages { get; set; } = new List<PrivateMessage>();
        public List<Event> CreatedEvents { get; set; } = new List<Event>();
        //public List<Comment> Comments { get; set; } = new List<Comment>(); clutter at have i User.
        public List<Post> Posts { get; set; } = new List<Post>();
        public List<UserSummary> Followers { get; set; } = new List<UserSummary>();
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
