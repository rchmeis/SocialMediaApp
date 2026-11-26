using MySql.Data.MySqlClient;
using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly IDbConnect _dbConnect;

        public LikeRepository(IDbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        /// <summary>
        /// Checks if a user has liked a specific post.
        /// </summary>
        /// <param name="key">The user and post composite key.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><c>true</c> if the user has liked the post; otherwise, <c>false</c>.</returns>
        public async Task<bool> IsLikedAsync(PostLike key, CancellationToken cancellationToken)
        {
            const string sql = """
            SELECT COUNT(*) FROM user_likes_post
            WHERE user_id = @user_id AND post_id = @post_id
            """;

            await using MySqlConnection connection = await _dbConnect.ConnectAsync(cancellationToken);
            await using MySqlCommand command = new(sql, connection);

            command.Parameters.AddWithValue("@user_id", key.UserId);
            command.Parameters.AddWithValue("@post_id", key.PostId);

            object result = await command.ExecuteScalarAsync(cancellationToken);

            return Convert.ToBoolean(result);
        }

        /// <summary>
        /// Adds a like to the specified post by the specified user.
        /// If the user has already liked the post, an InvalidOperationException is thrown.
        /// </summary>
        /// <param name="key">The post and user for which to add a like.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the like was added successfully.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the user has already liked the post.</exception>
        public async Task LikeAsync(PostLike key, CancellationToken cancellationToken)
        {
            const string sql = """
            INSERT INTO user_likes_post (user_id, post_id)
            VALUES (@user_id, @post_id)
            """;

            await using MySqlConnection connection = await _dbConnect.ConnectAsync(cancellationToken);
            await using MySqlCommand command = new(sql, connection);
            command.Parameters.AddWithValue("@user_id", key.UserId);
            command.Parameters.AddWithValue("@post_id", key.PostId);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException($"Failed to like the post with ID {key.PostId} for user with ID {key.UserId}.");
            }
        }

        /// <summary>
        /// Removes a like from the specified post by the specified user.
        /// If the user has not liked the post, an InvalidOperationException is thrown.
        /// </summary>
        /// <param name="key">The post and user for which to remove a like.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the like was removed successfully.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the user has not liked the post.</exception>
        public async Task UnlikeAsync(PostLike key, CancellationToken cancellationToken)
        {
            const string sql = """
            DELETE FROM user_likes_post
            WHERE user_id = @user_id AND post_id = @post_id
            """;

            await using MySqlConnection connection = await _dbConnect.ConnectAsync(cancellationToken);
            await using MySqlCommand command = new(sql, connection);
            command.Parameters.AddWithValue("@user_id", key.UserId);
            command.Parameters.AddWithValue("@post_id", key.PostId);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException($"Failed to unlike the post with ID {key.PostId} for user with ID {key.UserId}.");
            }
        }
    }
}
