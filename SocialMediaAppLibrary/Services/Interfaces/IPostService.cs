using Microsoft.AspNetCore.Http;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services.Interfaces
{
    public interface IPostService
    {
        public Task<Post> CreateAsync(Post entity, List<IFormFile> formFiles, CancellationToken cancellationToken);
        public Task<Post> DeleteAsync(int id, CancellationToken cancellationToken);
        public Task<Post?> ReadByIdAsync(int id, CancellationToken cancellationToken);
        public Task<GenericPaginatedResult<Post>> GetNewestFeedAsync(PaginationOptions queryOptions, CancellationToken cancellationToken);
        public Task<GenericPaginatedResult<Post>> GetFeedByIdAsync(int id, PaginationOptions queryOptions, CancellationToken cancellationToken);
        // public Task<GenericPaginatedResult<Post>> GetRecommendedFeedAsync(PaginationOptions queryOptions, CancellationToken cancellationToken);
    }
}
