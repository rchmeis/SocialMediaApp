using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public City City { get; set; }   //REMOVE CITYIF REST OF GROUP DONT APPROVE
        public Image Image { get; set; }
        public Institute? Institute { get; set; }
        public string? FieldOfStudy { get; set; }
        public User Creator { get; set; }
        public List<Partner>? Sponsors { get; set; } = new List<Partner>();
        public List<Student> AttendingStudents { get; set; } = new List<Student>();
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

    }
}
