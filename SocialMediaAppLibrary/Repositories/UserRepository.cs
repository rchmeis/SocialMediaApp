using MySql.Data.MySqlClient;
using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
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
        /// Retrieves a <see cref="UserSummary"/>—including display name and optional profile image—for
        /// the active user associated with the specified authentication account identifier.
        /// </summary>
        /// <param name="userAuthId">The unique identifier of the linked user authentication record.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// The matching non-deleted <see cref="UserSummary"/> if found; otherwise, <see langword="null"/>.
        /// </returns>
        public async Task<UserSummary?> GetByUserAuthIdAsync(int userAuthId, CancellationToken cancellationToken=default)
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
                    i.image_id,
                    i.file_name,
                    i.alt_text,
                    i.image_data,
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
                int imageDataOrdinal = reader.GetOrdinal("image_data");
                int imageCreatedAtOrdinal = reader.GetOrdinal("image_created_at");

                byte[] imageData = Array.Empty<byte>();

                if (! await reader.IsDBNullAsync(imageDataOrdinal, cancellationToken))
                {
                    long bytesLength = reader.GetBytes(imageDataOrdinal, 0, null, 0, 0); //first call: how many bytes are there? null->no destination array yet. 000, start reading at index 0, start writing at index 0, copy 0 bytes right now.
                    imageData = new byte[bytesLength]; //Allocate the array of that exact size
                    reader.GetBytes(imageDataOrdinal, 0, imageData, 0, (int)bytesLength); //Second call—actually copy the bytes into the array. (int)bytesLength: copy all bytesLength bytes.
                }

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
                image = image
            };

        }
    }
}
