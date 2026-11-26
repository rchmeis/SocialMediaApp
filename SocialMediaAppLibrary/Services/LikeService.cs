using System;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Services.Interfaces;

namespace SocialMediaAppLibrary.Services;

public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;

    public LikeService(ILikeRepository likeRepository)
    {
        _likeRepository = likeRepository;
    }

    /// <summary>
    /// Checks if a user has liked a specific post.
    /// </summary>
    /// <param name="key">The user and post composite key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the user has liked the post; otherwise, <c>false</c>.</returns>
    public async Task<bool> IsLikedAsync(PostLike key, CancellationToken cancellationToken)
    {
        return await _likeRepository.IsLikedAsync(key, cancellationToken);
    }

    /// <summary>
    /// Likes a post on behalf of a user. Throws an exception if the user has already liked the post.
    /// </summary>
    /// <param name="key">The user and post composite key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the user has already liked the post.</exception>
    public async Task LikeAsync(PostLike key, CancellationToken cancellationToken)
    {
        bool isLiked = await _likeRepository.IsLikedAsync(key, cancellationToken);
        if (isLiked)
        {
            throw new InvalidOperationException("User has already liked this post.");
        }

        await _likeRepository.LikeAsync(key, cancellationToken);
    }

    /// <summary>
    /// Unlikes a post on behalf of a user. Throws an exception if the user has not liked the post.
    /// </summary>
    /// <param name="key">The user and post composite key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the user has not liked the post.</exception>
    public async Task UnlikeAsync(PostLike key, CancellationToken cancellationToken)
    {
        bool isLiked = await _likeRepository.IsLikedAsync(key, cancellationToken);
        if (!isLiked)
        {
            throw new InvalidOperationException("User has not liked this post.");
        }

        await _likeRepository.UnlikeAsync(key, cancellationToken);
    }
}
