using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public enum Course
    {
        [Display(Name = "Datamatiker")]
        Datamatiker,

        [Display(Name = "Datalogi (Uni)")]
        Datalogi,

        [Display(Name = "Tandlæge")]
        Tandlaege, // Undgå æ/ø/å i selve koden for at slippe for encoding-problemer

        [Display(Name = "Sygeplejerske")]
        Sygeplejerske
    }
}
