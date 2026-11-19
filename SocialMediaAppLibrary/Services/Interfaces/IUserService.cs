using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services.Interfaces
{
    public interface IUserService
    {
        public UserSummary GetByUserAuthId(int id);
    }
}
