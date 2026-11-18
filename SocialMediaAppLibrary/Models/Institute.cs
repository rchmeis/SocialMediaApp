using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class Institute
    {
        public int Id { get; set; }
        public string DomainName { get; set; }
        public string Name { get; set; }
        public City City { get; set; }
        public List<Course> Courses { get; set; } = new List<Course>();
    }
}
