using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    public class UserRepository : IUserRepository
    {
        public Task<UserSummary> GetByUserAuthIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
