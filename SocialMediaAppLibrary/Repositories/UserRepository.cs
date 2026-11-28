using MySql.Data.MySqlClient;
using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    /// <summary>
    /// Provides MySQL data access operations for retrieving user profile summary information.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnect _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="dbconnect">The database connection provider used to open MySQL connections.</param>
        public UserRepository(IDbConnect dbconnect)
        {
            _db = dbconnect;
        }

        /// <summary>
        /// Retrieves the total number of users following the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user being followed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The number of follower relationships pointing to <paramref name="userId"/>.</returns>
        public async Task<int> GetFollowersCountAsync(int userId, CancellationToken cancellationToken)
        {
            const string sql = @"
                    SELECT COUNT(*)
                    FROM user_follows_user
                    WHERE to_user_id = @user_id;";

            await using MySqlConnection connection = _db.Connect();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@user_id", userId);

            object? result = await command.ExecuteScalarAsync();
            return result is null or DBNull ? 0 : Convert.ToInt32(result);
        }

        /// <summary>
        /// Retrieves the total number of users following the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user being followed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The number of follower relationships pointing to <paramref name="userId"/>.</returns>
        public async Task<int> GetFollowingCountAsync(int userId, CancellationToken cancellationToken)
        {
            const string sql = @"
                    SELECT COUNT(*)
                    FROM user_follows_user
                    WHERE from_user_id = @user_id;";

            await using MySqlConnection connection = _db.Connect();
           

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@user_id", userId);

            object? result = await command.ExecuteScalarAsync();
            return result is null or DBNull ? 0 : Convert.ToInt32(result);
        }

        public Task<User> WriteAsync(User entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public Task<User> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public async Task<User?> ReadByIdAsync(int id, CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return null;
            }
            await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);
            string query = """
                        SELECT
                            u.user_id,
                            u.user_auth_id,
                            u.bio,
                            u.image_id,
                            u.created_at,
                            u.deleted_at,
                            i.image_id      AS img_image_id,
                            i.file_name     AS img_file_name,
                            i.alt_text      AS img_alt_text,
                            -- i.image_data    AS img_image_data,  -- Commented out as Images PageModel will handle image data retrieval and caching
                            i.height        AS img_height,
                            i.width         AS img_width,
                            i.file_size     AS img_file_size,
                            i.created_at    AS img_created_at
                        FROM users u
                        LEFT JOIN images i
                            ON i.image_id = u.image_id
                        WHERE u.user_id = @userId
                          AND u.deleted_at IS NULL
                        LIMIT 1;
                        """;
            await using MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.Add("@userId", MySqlDbType.Int32).Value = id;

            await using DbDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return null;

            int ordUserId = reader.GetOrdinal("user_id");
            int ordUserAuthId = reader.GetOrdinal("user_auth_id");
            int ordBio = reader.GetOrdinal("bio");
            int ordImageId = reader.GetOrdinal("image_id");
            int ordCreatedAt = reader.GetOrdinal("created_at");
            int ordDeletedAt = reader.GetOrdinal("deleted_at");

            int ordImgId = reader.GetOrdinal("img_image_id");
            int ordFileName = reader.GetOrdinal("img_file_name");
            int ordAltText = reader.GetOrdinal("img_alt_text");
            // int ordImageData = reader.GetOrdinal("img_image_data"); // Commented out as Images PageModel will handle image data retrieval and caching
            int ordHeight = reader.GetOrdinal("img_height");
            int ordWidth = reader.GetOrdinal("img_width");
            int ordFileSize = reader.GetOrdinal("img_file_size");
            int ordImgCreatedAt = reader.GetOrdinal("img_created_at");

            Image? image = null;
            if (!await reader.IsDBNullAsync(ordImgId, cancellationToken))
            {
                /*byte[]? imageData = null; // Commented out as Images PageModel will handle image data retrieval and caching
                if (!await reader.IsDBNullAsync(ordImageData, cancellationToken))
                {
                    long bytesLength = reader.GetBytes(ordImageData, 0, null, 0, 0);
                    imageData = new byte[bytesLength];
                    reader.GetBytes(ordImageData, 0, imageData, 0, (int)bytesLength);
                }*/

                image = new Image
                {
                    Id = reader.GetInt32(ordImgId),
                    FileName = await reader.IsDBNullAsync(ordFileName, cancellationToken) ? string.Empty : reader.GetString(ordFileName),
                    AltText = await reader.IsDBNullAsync(ordAltText, cancellationToken) ? null : reader.GetString(ordAltText),
                    // ImageData = imageData, // Commented out as Images PageModel will handle image data retrieval and caching
                    Height = await reader.IsDBNullAsync(ordHeight, cancellationToken) ? 0 : reader.GetInt32(ordHeight),
                    Width = await reader.IsDBNullAsync(ordWidth, cancellationToken) ? 0 : reader.GetInt32(ordWidth),
                    FileSize = await reader.IsDBNullAsync(ordFileSize, cancellationToken) ? 0 : reader.GetInt32(ordFileSize),
                    CreatedAt = reader.GetDateTime(ordImgCreatedAt)
                };
            }

            return new User
            {
                Id = reader.GetInt32(ordUserId),
                UserAuthenticationId = reader.GetInt32(ordUserAuthId),
                Bio = await reader.IsDBNullAsync(ordBio, cancellationToken) ? null : reader.GetString(ordBio),
                Image = image,
                CreatedAt = reader.GetDateTime(ordCreatedAt),
                DeletedAt = await reader.IsDBNullAsync(ordDeletedAt, cancellationToken) ? null : reader.GetDateTime(ordDeletedAt)
            };

        }
        /// <summary>
        /// Retrieves a <see cref="UserSummary"/>—including display name and optional profile image—for
        /// the active user associated with the specified authentication account identifier.
        /// </summary>
        /// <param name="userAuthId">The unique identifier of the linked user authentication record.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// The matching non-deleted <see cref="UserSummary"/> if found; otherwise, <see langword="null"/>.
        /// </returns>
        public async Task<UserSummary?> GetSummaryByUserAuthIdAsync(int userAuthId, CancellationToken cancellationToken=default)
        {
            if (userAuthId <= 0)
            {
                return null;
            }
            await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);

            // COALESCE returns the first non-null value; CONCAT_WS joins non-null parts with a space.
            string query = """
                SELECT
                    u.user_id AS id,
                    COALESCE(
                        p.name_of_organization,
                        CONCAT_WS(' ', s.first_name, s.last_name)
                    ) AS display_name,
                    COALESCE(s.is_onboarded, 1) AS is_onboarded,   -- new: partners/admins have no student row → treat as onboarded
                    i.image_id,
                    i.file_name,
                    i.alt_text,
                    -- i.image_data,                               -- Commented out as Images PageModel will handle image data retrieval and caching
                    i.created_at AS image_created_at
                FROM users u
                LEFT JOIN partners p ON p.user_id = u.user_id
                LEFT JOIN students s ON s.user_id = u.user_id
                LEFT JOIN images i ON i.image_id = u.image_id
                WHERE u.user_auth_id = @userAuthId
                  AND u.deleted_at IS NULL
                LIMIT 1;
                """;

            await using MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userAuthId", userAuthId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            int displayNameOrdinal = reader.GetOrdinal("display_name");
            int imageIdOrdinal = reader.GetOrdinal("image_id");
            int idOrdinal = reader.GetOrdinal("id");
        

            Image? image = null;    
            if (!await reader.IsDBNullAsync(imageIdOrdinal, cancellationToken))
            {
                // A user may not have a profile image (u.image_id IS NULL), so i.image_id can be DBNull.
                int fileNameOrdinal = reader.GetOrdinal("file_name");
                int altTextOrdinal = reader.GetOrdinal("alt_text");
                // int imageDataOrdinal = reader.GetOrdinal("image_data"); // Commented out as Images PageModel will handle image data retrieval and caching
                int imageCreatedAtOrdinal = reader.GetOrdinal("image_created_at");

                byte[] imageData = Array.Empty<byte>();

                /*if (! await reader.IsDBNullAsync(imageDataOrdinal, cancellationToken)) // Commented out as Images PageModel will handle image data retrieval and caching
                {
                    long bytesLength = reader.GetBytes(imageDataOrdinal, 0, null, 0, 0); //first call: how many bytes are there? null->no destination array yet. 000, start reading at index 0, start writing at index 0, copy 0 bytes right now.
                    imageData = new byte[bytesLength]; //Allocate the array of that exact size
                    reader.GetBytes(imageDataOrdinal, 0, imageData, 0, (int)bytesLength); //Second call—actually copy the bytes into the array. (int)bytesLength: copy all bytesLength bytes.
                }*/

                image = new Image
                {
                    Id = reader.GetInt32(imageIdOrdinal),
                    FileName = reader.GetString(fileNameOrdinal),
                    AltText = reader.GetString(altTextOrdinal),
                    ImageData = imageData,
                    CreatedAt = reader.GetDateTime(imageCreatedAtOrdinal)
                };
            }

            return new UserSummary
            {
                Id = reader.GetInt32(idOrdinal),
                DisplayName = await reader.IsDBNullAsync(displayNameOrdinal, cancellationToken)
                    ? string.Empty
                    : reader.GetString(displayNameOrdinal),
                ProfileImage = image
            };

        }

        public Task<User> UpdateAsync(int id, User entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        public Task<List<User>> ReadAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether the signed-in user currently follows the target user.
        /// </summary>
        /// <param name="loggedInUserId">The unique identifier of the user who may be following.</param>
        /// <param name="targetUserId">The unique identifier of the user who may be followed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// <see langword="true"/> if a follow relationship exists from <paramref name="loggedInUserId"/> to
        /// <paramref name="targetUserId"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public async Task<bool> IsFollowingAsync(int loggedInUserId, int targetUserId, CancellationToken cancellationToken)
        {
            if (loggedInUserId == targetUserId)
            {
                return false;
            }

            //select 1 tells mysql not to read or send actual column data - just return the number 1.
            const string sql = @"
                    SELECT 1
                    FROM user_follows_user
                    WHERE from_user_id = @from_user_id
                      AND to_user_id = @to_user_id
                    LIMIT 1;";

            await using MySqlConnection connection = _db.Connect();

            await using MySqlCommand command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@from_user_id", loggedInUserId);
            command.Parameters.AddWithValue("@to_user_id", targetUserId);

            //ExecuteScalar returns the first column of the first row in the result set. NO MATCHING ROW -> returns null. IF a matching row exists but the first column value is SQL NULL the value is represented as DBNull.
            //We used select 1, so it will return 1 if it exists but null if the row doesnt exist, and DBNull if the value in the first column is SQL NULL
            object? result = await command.ExecuteScalarAsync(cancellationToken);         
            return result is not null and not DBNull;
        }

        /// <summary>
        /// Creates a follow relationship from the signed-in user to the target user if it does not already exist.
        /// </summary>
        /// <param name="loggedInUserId">The unique identifier of the user initiating the follow.</param>
        /// <param name="targetUserId">The unique identifier of the user being followed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// <see langword="true"/> if a new follow row was inserted; otherwise, <see langword="false"/>.
        /// </returns>
        public async Task<bool> FollowAsync(int loggedInUserId, int targetUserId, CancellationToken cancellationToken)
        {
            if (loggedInUserId == targetUserId)
            {
                return false;
            }

            //Insert ignore - hvis der allerede findes en række med (from_user_id, to_user_id) returnerer den bare rowsaffected=0.
            const string sql = @"
                    INSERT IGNORE INTO user_follows_user (from_user_id, to_user_id)
                    VALUES (@from_user_id, @to_user_id);";

            await using MySqlConnection connection = _db.Connect();

            await using MySqlCommand command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@from_user_id", loggedInUserId);
            command.Parameters.AddWithValue("@to_user_id", targetUserId);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Removes an existing follow relationship from the signed-in user to the target user.
        /// </summary>
        /// <param name="loggedInUserId">The unique identifier of the user unfollowing.</param>
        /// <param name="targetUserId">The unique identifier of the user being unfollowed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// <see langword="true"/> if a follow row was deleted; otherwise, <see langword="false"/>.
        /// </returns>
        public async Task<bool> UnfollowAsync(int loggedInUserId, int targetUserId, CancellationToken cancellationToken)
        {
            if (loggedInUserId == targetUserId)
            {
                return false;
            }

            const string sql = @"
                    DELETE FROM user_follows_user
                    WHERE from_user_id = @from_user_id
                    AND to_user_id = @to_user_id;";

            await using MySqlConnection connection = _db.Connect();

            await using MySqlCommand command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@from_user_id", loggedInUserId);
            command.Parameters.AddWithValue("@to_user_id", targetUserId);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }
    }
}
