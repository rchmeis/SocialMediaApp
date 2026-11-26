using System;

namespace SocialMediaAppLibrary.Models;

/// <summary>
/// Represents a composite key for the <c>user_likes_post</c> table, consisting of the user ID and the post ID.
/// </summary>
public sealed record PostLike
{
    /// <summary>
    /// Gets the ID of the user who liked the post.
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// Gets the ID of the post that was liked.
    /// </summary>
    public int PostId { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostLike"/> record.
    /// </summary>
    public PostLike() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostLike"/> record with the specified user ID and post ID.
    /// </summary>
    /// <param name="userId">The ID of the user who liked the post.</param>
    /// <param name="postId">The ID of the post that was liked.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="userId"/> or <paramref name="postId"/> is negative.</exception>
    public PostLike(int userId, int postId)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(userId, nameof(userId));
        ArgumentOutOfRangeException.ThrowIfNegative(postId, nameof(postId));

        UserId = userId;
        PostId = postId;
    }

    public override string ToString()
    {
        return $"UserId: {UserId}, PostId: {PostId}";
    }
}
