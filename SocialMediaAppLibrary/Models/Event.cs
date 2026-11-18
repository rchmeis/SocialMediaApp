using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class Event
    {
        public int Id { get; set; }
        public UserSummary Creator { get; set; }
        public string Title { get; set; }
        public City City { get; set; } 
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }       
        public List<Image> Images { get; set; } = new List<Image>();
        public Institute? Institute { get; set; }
        public string? FieldOfStudy { get; set; }        
        public bool IsSponsored { get; set; } = false;
        public Dictionary<int, UserSummary> Attendees { get; set; } = new Dictionary<int, UserSummary>();
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
