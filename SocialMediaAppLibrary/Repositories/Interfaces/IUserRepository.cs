using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<UserSummary> GetByUserAuthIdAsync(int id, CancellationToken cancellationToken);        
    }
}
