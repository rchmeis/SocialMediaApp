using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Models
{
    public readonly record struct DuplicateAccountCheck(bool EmailExists, bool UsernameExists);
    
}
