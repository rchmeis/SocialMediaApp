using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services.Interfaces
{
    public interface IUserService:IUserRepository
    {
        public Task<UserSummary> GetByUserAuthIdAsync(int id, CancellationToken cancellationToken);
    }
}
