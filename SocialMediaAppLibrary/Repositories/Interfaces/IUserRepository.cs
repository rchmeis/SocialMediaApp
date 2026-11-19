using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public UserSummary GetByUserAuthId(int id);
    }
}
