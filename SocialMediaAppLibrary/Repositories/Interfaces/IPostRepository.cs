using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public interface IPostRepository : ICrud<Post, int>
    {
        public Task<GenericPaginatedResult<Post>> ReadNewestFeedAsync(PaginationOptions paginationOptions, CancellationToken cancellationToken);
        public Task<GenericPaginatedResult<Post>> ReadRecommendedFeedAsync(PaginationOptions paginationOptions, CancellationToken cancellationToken);
        public Task<GenericPaginatedResult<Post>> ReadFeedByIdAsync(int id, PaginationOptions paginationOptions, CancellationToken cancellation);
    }
}
