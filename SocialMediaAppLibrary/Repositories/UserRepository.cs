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
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnect _db;
        public UserRepository(IDbConnect dbconnect)
        {
            _db = dbconnect;
        }
        public async Task<UserSummary> GetByUserAuthIdAsync(int userAuthId)
        {
            if (userAuthId <= 0)
            {
                return null;
            }
            using MySqlConnection connection = _db.Connect();

            //COALESCE(value1, value2, ...) returns the first non-null value in the list, evaluating from left to right.
            //CONCAT_WS stands for Concatenate With Separator. Its first argument is the separator string, followed by the values to join:
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

            using MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userAuthId", userAuthId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            int displayNameOrdinal = reader.GetOrdinal("display_name");
            int imageIdOrdinal = reader.GetOrdinal("image_id");

            Image? image = null;    
            if (!reader.IsDBNull(imageIdOrdinal))
            {
                ////A user in users might not have a profile picture (u.image_id is NULL)
                //Because we used LEFT JOIN images, i.image_id will be NULL in that case. If there's no image, we leave image as null and skip reading image columns.
                int imageDataOrdinal = reader.GetOrdinal("image_data"); 
                byte[] imageData = Array.Empty<byte>();

                if (!reader.IsDBNull(imageDataOrdinal))
                {
                    long bytesLength = reader.GetBytes(imageDataOrdinal, 0, null, 0, 0); //first call: how many bytes are there? null->no destination array yet. 000, start reading at index 0, start writing at index 0, copy 0 bytes right now.
                    imageData = new byte[bytesLength]; //Allocate the array of that exact size
                    reader.GetBytes(imageDataOrdinal, 0, imageData, 0, (int)bytesLength); //Second call—actually copy the bytes into the array. (int)bytesLength: copy all bytesLength bytes.
                }

                image = new Image
                {
                    Id = reader.GetInt32(imageIdOrdinal),
                    FileName = reader.GetString("file_name"),
                    AltText = reader.GetString("alt_text"),
                    ImageData = imageData,
                    CreatedAt = reader.GetDateTime("image_created_at")
                };
            }

            return new UserSummary
            {
                Id = reader.GetInt32("userAuthId"),
                DisplayName = reader.IsDBNull(displayNameOrdinal)
                  ? string.Empty
                  : reader.GetString(displayNameOrdinal),
                image = image
            };

        }
    }
}
