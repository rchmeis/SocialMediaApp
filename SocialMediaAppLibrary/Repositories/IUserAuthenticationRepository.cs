using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    public interface IUserAuthenticationRepository: ICrud<UserAuthentication, int>
    {
    }
}
