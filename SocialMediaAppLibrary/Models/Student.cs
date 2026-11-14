using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class Student
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsOnboarded { get; set; }
        public Course Course { get; set; }
        public Institute Institute { get; set; }
        public List<Event> AttendingEvents { get; set; } = new List<Event>();
        public List<string> Interests { get; set; } = new List<string>();
        public List<User> Following { get; set; } = new List<User>();        
        public List<User> BlockedUsers { get; set; } = new List<User>();

    }
}
