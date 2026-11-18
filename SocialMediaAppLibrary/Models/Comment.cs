using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public UserSummary Author { get; set; }
        public string Content { get; set; }
        public Post post { get; set; }
        public List<UserSummary> LikedBy { get; set; } = new List<UserSummary>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
