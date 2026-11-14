using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class Partner:User
    {
        public string NameOfOrganisation { get; set; }
        public List<StudentJob> StudentJobs { get; set; }=new List<StudentJob>();
    }
}
