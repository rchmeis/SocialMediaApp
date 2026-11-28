using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public interface IUserRepository : ICrud<User, int>
    {
        public Task<UserSummary> GetSummaryByUserAuthIdAsync(int id, CancellationToken cancellationToken);
        Task<int> GetFollowersCountAsync(int userId, CancellationToken cancellationToken);
        Task<int> GetFollowingCountAsync(int userId, CancellationToken cancellationToken);
        public Task<bool>IsFollowingAsync(int loggedInUserId, int targetUserId, CancellationToken cancellationToken);
        public Task<bool> FollowAsync(int loggedInUserId, int targetUserId, CancellationToken cancellation);
        public Task<bool> UnfollowAsync(int loggedInUserId, int targetUserId, CancellationToken cancellationToken);
        public Task<List<UserSummary>> GetSummariesByIdsAsync(List<int> userIds, CancellationToken cancellationToken);

    }
}
