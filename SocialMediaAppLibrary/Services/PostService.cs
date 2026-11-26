using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Services.Interfaces;
using ImageSharp = SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace SocialMediaAppLibrary.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;

        private readonly IImageService _imageService;

        public PostService(IPostRepository postRepository, IImageService imageService)
        {
            _postRepository = postRepository;
            _imageService = imageService;
        }

        /// <summary>
        /// Creates a new post and processes the associated image files, optimizing them and adding them to the post's Images collection.
        /// </summary>
        /// <param name="post">The post to create.</param>
        /// <param name="formFiles">The image files to process.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The created post.</returns>
        public async Task<Post> CreateAsync(Post post, List<IFormFile> formFiles, CancellationToken cancellationToken)
        {
            // Process each image file, optimize it, and add it to the post's Images collection
            foreach (IFormFile formFile in formFiles)
            {
                Image optimizedImage = await _imageService.OptimizeImageAsync(formFile);
                optimizedImage.AltText = optimizedImage.FileName; // Hack: Set AltText to the file name, as we don't have a separate alt text input
                post.Images.Add(optimizedImage);
            }

            return await _postRepository.WriteAsync(post, cancellationToken);
        }

      

        public async Task<Post> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            return await _postRepository.DeleteAsync(id, cancellationToken);
        }

        /// <summary>
        /// Retrieves a paginated feed of posts for the specified user.
        /// </summary>
        /// <param name="id">The identifier of the user whose post feed is being requested.</param>
        /// <param name="queryOptions">The pagination parameters specifying page number and page size.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation if the request aborts.</param>
        /// <returns>
        /// A <see cref="GenericPaginatedResult{T}"/> of <see cref="Post"/> containing the posts for the requested page and pagination metadata.
        /// </returns>
        public Task<GenericPaginatedResult<Post>> GetFeedByIdAsync(int id, PaginationOptions queryOptions, CancellationToken cancellationToken)
        {
            return _postRepository.ReadFeedByIdAsync(id, queryOptions, cancellationToken);
        }

        /// <summary>
        /// Retrieves a list of posts for the news feed, paginated based on the provided query options.
        /// The method fetches posts along with their associated author and images, ordered by creation date
        /// </summary>
        /// <param name="paginationOptions">The pagination options</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A list of posts for the news feed</returns>
        public async Task<GenericPaginatedResult<Post>> GetNewestFeedAsync(PaginationOptions paginationOptions, CancellationToken cancellationToken)
        {
            return await _postRepository.ReadNewestFeedAsync(paginationOptions, cancellationToken);
        }

        public List<Post> ReadAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves a post by its unique identifier from the database, including its associated author and images.
        /// </summary>
        /// <param name="id">The unique identifier of the post to retrieve.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The retrieved post, or null if not found.</returns>
        public async Task<Post?> ReadByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _postRepository.ReadByIdAsync(id, cancellationToken);
        }
    }
}
