using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class Student: User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsOnboarded { get; set; }
        public Course Course { get; set; }
        public Institute Institute { get; set; }
        public List<string> Interests { get; set; } = new List<string>();
    }
}
