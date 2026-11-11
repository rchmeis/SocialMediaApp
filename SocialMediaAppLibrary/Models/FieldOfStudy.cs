using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public enum FieldOfStudy
    {
        Teknologi,
        Design,
        Business,
        Sundhedsvidenskab,
        Naturvidenskab,
        Samfundsvidenskab,
        Humaniora,
        [Display(Name = "Ingeniør")]
        Ingenior
    }
}
