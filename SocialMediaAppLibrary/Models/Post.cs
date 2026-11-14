using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class Post
    {
        public int Id { get; set; }
        public StudentDTO Author { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; }        
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public int Likes { get; set; } = 0;
        public List<Image>? Images { get; set; } = new List<Image>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

    }
}
