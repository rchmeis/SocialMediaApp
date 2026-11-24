using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public interface IPostRepository : ICrud<Post, int>
    {
        public Task<List<Post>> ReadNewsFeedAsync(PaginationOptions paginationOptions, CancellationToken cancellationToken);
        public Task<List<Post>> ReadRecommendedFeedAsync(PaginationOptions paginationOptions, CancellationToken cancellationToken);
    }
}
