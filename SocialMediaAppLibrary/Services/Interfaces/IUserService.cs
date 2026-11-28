using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services.Interfaces
{
    public interface IUserService:IUserRepository
    {
        public Task<UserSummary> GetSummaryByUserAuthIdAsync(int id, CancellationToken cancellationToken);
        public Task<bool> IsFollowingAsync(int loggedInUserId, int targetUserId, CancellationToken cancellationToken);
        public Task<bool> FollowAsync(int loggedInUserId, int targetUserId, CancellationToken cancellation);
        public Task<bool> UnfollowAsync(int loggedInUserId, int targetUserId, CancellationToken cancellationToken);
    }
}
