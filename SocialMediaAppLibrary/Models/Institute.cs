using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public class Institute
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public City city { get; set; } //REMOVE IF REST OF GROUP DONT APPROVE
    }
}
