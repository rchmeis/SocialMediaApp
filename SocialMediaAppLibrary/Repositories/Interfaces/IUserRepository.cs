using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public interface IUserRepository : ICrud<User, int>
    {
        public Task<UserSummary> GetByUserAuthIdAsync(int id, CancellationToken cancellationToken);
        Task<int> GetFollowersCountAsync(int userId, CancellationToken cancellationToken);
        Task<int> GetFollowingCountAsync(int userId, CancellationToken cancellationToken);
    }
}
