using MySql.Data.MySqlClient;
using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
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

        public Image Delete(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves all images from the database and returns them as a list of <see cref="Image"/> objects.
        /// </summary>
        /// <returns>A list of Image objects.</returns>
        public List<Image> ReadAll()
        {
            const string sql = """
            SELECT image_id, file_name, alt_text, image_data, height, width, file_size, created_at
            FROM images;
            """;

            List<Image> images = new List<Image>();

            using MySqlConnection connection = _dbConnect.Connect();

            using MySqlCommand command = new MySqlCommand(sql, connection);

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                images.Add(new Image
                {
                    Id = reader.GetInt32("image_id"),
                    FileName = reader.GetString("file_name"),
                    AltText = reader.GetString("alt_text"),
                    ImageData = (byte[])reader["image_data"],
                    Height = reader.GetInt32("height"),
                    Width = reader.GetInt32("width"),
                    FileSize = reader.GetInt32("file_size"),
                    CreatedAt = reader.GetDateTime("created_at")
                });
            }

            return images;
        }

        /// <summary>
        /// Retrieves an image from the database by its ID and returns it as an <see cref="Image"/> object. If no image is found with the given ID, returns null.
        /// </summary>
        /// <param name="id">The ID of the image to retrieve.</param>
        /// <returns>The Image object if found, otherwise null.</returns>
        public Image? ReadById(int id)
        {
            const string sql = """
            SELECT image_id, file_name, alt_text, image_data, height, width, file_size, created_at
            FROM images
            WHERE image_id = @id;
            """;

            using MySqlConnection connection = _dbConnect.Connect();

            using MySqlCommand command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using MySqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Image
                {
                    Id = reader.GetInt32("image_id"),
                    FileName = reader.GetString("file_name"),
                    AltText = reader.GetString("alt_text"),
                    ImageData = (byte[])reader["image_data"],
                    Height = reader.GetInt32("height"),
                    Width = reader.GetInt32("width"),
                    FileSize = reader.GetInt32("file_size"),
                    CreatedAt = reader.GetDateTime("created_at")
                };
            }

            return null; // Return null if no image is found with the given ID
        }

        public Task<Image?> ReadByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Image Update(int id, Image entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves an image to the database and returns the saved image with its generated ID and creation timestamp.
        /// </summary>
        /// <param name="image">The image to save.</param>
        /// <returns>The saved image with its generated ID and creation timestamp.</returns>
        public Image Write(Image image)
        {
            const string sql = """
            INSERT INTO images (file_name, alt_text, image_data, height, width, file_size)
            VALUES (@file_name, @alt_text, @image_data, @height, @width, @file_size);
            
            SELECT image_id, created_at
            FROM images
            WHERE image_id = LAST_INSERT_ID();
            """;

            using MySqlConnection connection = _dbConnect.Connect();

            using MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@file_name", image.FileName);
            command.Parameters.AddWithValue("@image_data", image.ImageData);
            command.Parameters.AddWithValue("@alt_text", image.AltText);
            command.Parameters.AddWithValue("@height", image.Height);
            command.Parameters.AddWithValue("@width", image.Width);
            command.Parameters.AddWithValue("@file_size", image.FileSize);

            using MySqlDataReader reader = command.ExecuteReader();

            // Populate with values generated by the database (image_id and created_at)
            if (reader.Read())
            {
                image.Id = reader.GetInt32("image_id");
                image.CreatedAt = reader.GetDateTime("created_at");
            }

            return image;
        }

        public Task<Image> WriteAsync(Image entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
