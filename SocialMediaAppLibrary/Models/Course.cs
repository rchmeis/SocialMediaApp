using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FieldOfStudy { get; set; }
    }
}
