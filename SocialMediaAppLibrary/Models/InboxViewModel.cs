using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class InboxViewModel
    {
        public List<PrivateMessage> ReceivedMessages { get; set; } = new List<PrivateMessage>();
        public List<PrivateMessage> SentMessages { get; set; } = new List<PrivateMessage>();
        public int UnreadCount { get; set; } = 0;
    }
}
