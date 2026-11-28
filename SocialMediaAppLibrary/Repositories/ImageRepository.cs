using MySql.Data.MySqlClient;
using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly IDbConnect _dbConnect;

        public ImageRepository(IDbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }
        
        /// <summary>
        /// Deletes the image with the specified identifier and updates the <c>updated_at</c> timestamp
        /// of the post it belongs to. Both changes run in a single transaction, so either both are saved or neither is.
        /// </summary>
        /// <remarks>
        /// The <c>post_images</c> row is removed automatically by the database (<c>ON DELETE CASCADE</c>).
        /// The post is updated before the image is deleted, because the link to the post is gone afterwards.
        /// </remarks>
        /// <param name="imageId">The unique identifier of the image to delete.</param>
        /// <param name="cancellationToken">A token to cancel the operation, e.g. if the request is aborted.</param>
        /// <returns>An <see cref="Image"/> containing only the <see cref="Image.Id"/> of the deleted image.</returns>
        /// <exception cref="Exception">Thrown when the image could not be deleted. The transaction is rolled back.</exception>
        public async Task<Image> DeleteAsync(int imageId, CancellationToken cancellationToken)
        {
            await using MySqlConnection connection = await _dbConnect.ConnectAsync();
            await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
            
            try
            {
                // For next time, update sql string so we left join post
                // so "updated_at" gets updated when we delete a post 
                string sqlUpdatePost = """
                                       UPDATE posts
                                        JOIN post_images ON posts.post_id = post_images.post_id
                                       SET updated_at = @updated_at
                                       WHERE post_images.image_id = @image_id
                                       """;
                
                string sqlDelete = """
                             DELETE FROM images 
                             WHERE image_id = @image_id
                             """;

                await using MySqlCommand cmdUpdate = new MySqlCommand(sqlUpdatePost, connection, transaction);
                cmdUpdate.Parameters.AddWithValue("updated_at", DateTime.Now);
                cmdUpdate.Parameters.AddWithValue("image_id", imageId);
                await cmdUpdate.ExecuteNonQueryAsync();

                await using MySqlCommand cmdDelete = new MySqlCommand(sqlDelete, connection, transaction);
                cmdDelete.Parameters.AddWithValue("image_id", imageId);
                await cmdDelete.ExecuteNonQueryAsync();

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new Exception("Coudn't delete image", ex);
            }

            return new Image { Id = imageId };
        }


        /// <summary>
        /// Retrieves all images from the database and returns them as a list of <see cref="Image"/> objects.
        /// </summary>
        /// <returns>A list of Image objects.</returns>
        public async Task<List<Image>> ReadAllAsync(CancellationToken cancellationToken)
        {
            const string sql = """
            SELECT image_id, file_name, alt_text, height, width, file_size, created_at
            FROM images;
            """;

            List<Image> images = new List<Image>();

            await using MySqlConnection connection = await _dbConnect.ConnectAsync();

            await using MySqlCommand command = new MySqlCommand(sql, connection);

            await using DbDataReader reader = await command.ExecuteReaderAsync();

            while (reader.Read())
            {
                images.Add(new Image
                {
                    Id = reader.GetInt32(reader.GetOrdinal("image_id")),
                    FileName = reader.GetString(reader.GetOrdinal("file_name")),
                    AltText = reader.GetString(reader.GetOrdinal("alt_text")),
                    Height = reader.GetInt32(reader.GetOrdinal("height")),
                    Width = reader.GetInt32(reader.GetOrdinal("width")),
                    FileSize = reader.GetInt32(reader.GetOrdinal("file_size")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                });
            }

            return images;
        }

        /// <summary>
        /// Retrieves an image from the database by its ID and returns it as an <see cref="Image"/> object. If no image is found with the given ID, returns null.
        /// </summary>
        /// <param name="id">The ID of the image to retrieve.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The Image object if found, otherwise null.</returns>
        public async Task<Image?> ReadByIdAsync(int id, CancellationToken cancellationToken)
        {
            const string sql = """
            SELECT image_id, file_name, alt_text, image_data, height, width, file_size, created_at
            FROM images
            WHERE image_id = @id;
            """;

            await using MySqlConnection connection = await _dbConnect.ConnectAsync();

            await using MySqlCommand command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            await using DbDataReader reader = await command.ExecuteReaderAsync();

            if (reader.Read())
            {
                return new Image
                {
                    Id = reader.GetInt32(reader.GetOrdinal("image_id")),
                    FileName = reader.GetString(reader.GetOrdinal("file_name")),
                    AltText = reader.GetString(reader.GetOrdinal("alt_text")),
                    ImageData = (byte[])reader[reader.GetOrdinal("image_data")],
                    Height = reader.GetInt32(reader.GetOrdinal("height")),
                    Width = reader.GetInt32(reader.GetOrdinal("width")),
                    FileSize = reader.GetInt32(reader.GetOrdinal("file_size")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                };
            }

            return null; // Return null if no image is found with the given ID
        }


        public Task<Image> UpdateAsync(int id, Image entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves an image to the database and returns the saved image with its generated ID and creation timestamp.
        /// </summary>
        /// <param name="image">The image to save.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The saved image with its generated ID and creation timestamp.</returns>
        public async Task<Image> WriteAsync(Image image, CancellationToken cancellationToken)
        {
            const string sql = """
            INSERT INTO images (file_name, alt_text, image_data, height, width, file_size)
            VALUES (@file_name, @alt_text, @image_data, @height, @width, @file_size);
            
            SELECT image_id, created_at
            FROM images
            WHERE image_id = LAST_INSERT_ID();
            """;

            await using MySqlConnection connection = await _dbConnect.ConnectAsync();

            await using MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@file_name", image.FileName);
            command.Parameters.AddWithValue("@image_data", image.ImageData);
            command.Parameters.AddWithValue("@alt_text", image.AltText);
            command.Parameters.AddWithValue("@height", image.Height);
            command.Parameters.AddWithValue("@width", image.Width);
            command.Parameters.AddWithValue("@file_size", image.FileSize);

            await using DbDataReader reader = await command.ExecuteReaderAsync();

            // Populate with values generated by the database (image_id and created_at)
            if (reader.Read())
            {
                image.Id = reader.GetInt32(reader.GetOrdinal("image_id"));
                image.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
            }

            return image;
        }
    }
}
