using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class Post
    {
        /// <summary>
        /// Gets or sets the unique identifier for the post.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the author of the post, represented as a <see cref="UserSummary"/> object.
        /// </summary>
        public UserSummary Author { get; set; } = new UserSummary();

        /// <summary>
        /// Gets or sets the title of the post.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the content of the post.
        /// </summary>
        public string Content { get; set; } = string.Empty; 

        /// <summary>
        /// Gets or sets the list of comments associated with the post.
        /// </summary>      
        public List<Comment> Comments { get; set; } = new List<Comment>();

        /// <summary>
        /// Gets or sets the number of likes the post has received.
        /// </summary>
        public int LikesCount { get; set; } = 0;

        // public Dictionary<int, UserSummary> LikedBy { get; set; } = new Dictionary<int, UserSummary>();

        /// <summary>
        /// Gets or sets the list of images associated with the post.
        /// </summary>
        public List<Image> Images { get; set; } = new List<Image>();

        /// <summary>
        /// Gets or sets the date and time when the post was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the post was last updated. This property is nullable, indicating that a post may not have been updated since its creation.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the post was soft deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Post"/> class.
        /// </summary>
        public Post() { }
    }
}
