using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    public class UserRepository : IUserRepository
    {
        public UserSummary GetByUserAuthId(int id)
        {
            throw new NotImplementedException();
        }
    }
}
