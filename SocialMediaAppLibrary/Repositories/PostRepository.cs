using SocialMediaAppLibrary.Database;
using System;
using System.Collections.Generic;
using System.Text;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace SocialMediaAppLibrary.Repositories
{
    public class PostRepository : IPostRepository
    {

        private readonly IDbConnect _dbConnect;

        public PostRepository(IDbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        public Task<Post> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<Post>> ReadAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves a post by its unique identifier from the database, including its associated author and images.
        /// </summary>
        /// <param name="id">The unique identifier of the post to retrieve.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The retrieved post, or null if not found.</returns>
        public async Task<Post?> ReadByIdAsync(int id, CancellationToken cancellationToken)
        {
            const string sql = """
            SELECT
                p.post_id,
                p.author_id,
                p.title,
                p.content,
                p.created_at,
                p.updated_at,

                COALESCE(likes.likes_count, 0) AS likes_count,

                u.user_id,
                CONCAT_WS(' ', s.first_name, s.last_name) AS display_name,

                user_image.image_id   AS profile_image_id,
                user_image.file_name  AS profile_image_file_name,
                user_image.alt_text   AS profile_image_alt_text,
                user_image.image_data AS profile_image_data,
                user_image.height     AS profile_image_height,
                user_image.width      AS profile_image_width,

                i.image_id   AS post_image_id,
                i.file_name  AS post_image_file_name,
                i.alt_text   AS post_image_alt_text,
                i.image_data AS post_image_data,
                i.height     AS post_image_height,
                i.width      AS post_image_width,
                i.file_size  AS post_image_file_size,
                i.created_at AS post_image_created_at

            FROM posts p

            LEFT JOIN (
                SELECT
                    post_id,
                    COUNT(*) AS likes_count
                FROM user_likes_post
                GROUP BY post_id
            ) likes ON p.post_id = likes.post_id                             -- Join the user_likes_post junction table to get the count of likes for each post with a subquery to avoid unnecessary row multiplication

            LEFT JOIN post_images pi ON p.post_id = pi.post_id               -- Join the post_images junction table to get the associated images for the post

            LEFT JOIN images i ON pi.image_id = i.image_id                   -- Join the images table to get the details of each image associated with the post

            LEFT JOIN users u ON p.author_id = u.user_id                     -- Join the users table to get the author details for the post

            LEFT JOIN students AS s ON u.user_id = s.user_id                 -- Join the students table to get the display name of the author

            LEFT JOIN images user_image ON u.image_id = user_image.image_id  -- Join the images table to get the profile image of the author

            WHERE p.post_id = @post_id AND p.deleted_at IS NULL;
            """;

            await using MySqlConnection connection = await _dbConnect.ConnectAsync();
            await using MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@post_id", id);

            await using DbDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            // Cache column ordinals
            int postIdOrdinal = reader.GetOrdinal("post_id");
            int userIdOrdinal = reader.GetOrdinal("user_id");
            int displayNameOrdinal = reader.GetOrdinal("display_name");
            int titleOrdinal = reader.GetOrdinal("title");
            int contentOrdinal = reader.GetOrdinal("content");
            int likesCountOrdinal = reader.GetOrdinal("likes_count");
            int createdAtOrdinal = reader.GetOrdinal("created_at");
            int updatedAtOrdinal = reader.GetOrdinal("updated_at");

            int profileImageIdOrdinal = reader.GetOrdinal("profile_image_id");
            int profileImageFileNameOrdinal = reader.GetOrdinal("profile_image_file_name");
            int profileImageAltTextOrdinal = reader.GetOrdinal("profile_image_alt_text");
            int profileImageDataOrdinal = reader.GetOrdinal("profile_image_data");
            int profileImageHeightOrdinal = reader.GetOrdinal("profile_image_height");
            int profileImageWidthOrdinal = reader.GetOrdinal("profile_image_width");

            int postImageIdOrdinal = reader.GetOrdinal("post_image_id");
            int postImageFileNameOrdinal = reader.GetOrdinal("post_image_file_name");
            int postImageAltTextOrdinal = reader.GetOrdinal("post_image_alt_text");
            int postImageDataOrdinal = reader.GetOrdinal("post_image_data");
            int postImageHeightOrdinal = reader.GetOrdinal("post_image_height");
            int postImageWidthOrdinal = reader.GetOrdinal("post_image_width");
            int postImageFileSizeOrdinal = reader.GetOrdinal("post_image_file_size");
            int postImageCreatedAtOrdinal = reader.GetOrdinal("post_image_created_at");

            // Populate the author's profile image if one exists
            Image? profileImage = null;
            if (!reader.IsDBNull(profileImageIdOrdinal))
            {
                profileImage = new Image
                {
                    Id = reader.GetInt32(profileImageIdOrdinal),
                    FileName = reader.GetString(profileImageFileNameOrdinal),
                    AltText = reader.GetString(profileImageAltTextOrdinal),
                    ImageData = (byte[])reader.GetValue(profileImageDataOrdinal),
                    Height = reader.GetInt32(profileImageHeightOrdinal),
                    Width = reader.GetInt32(profileImageWidthOrdinal)
                };
            }

            // Populate the author's details
            UserSummary author = new UserSummary
            {
                Id = reader.GetInt32(userIdOrdinal),
                DisplayName = reader.GetString(displayNameOrdinal),
                ProfileImage = profileImage
            };

            // Populate the post's details
            Post post = new Post
            {
                Id = reader.GetInt32(postIdOrdinal),
                Author = author,
                Title = reader.GetString(titleOrdinal),
                Content = reader.GetString(contentOrdinal),
                LikesCount = Convert.ToInt32(reader.GetValue(likesCountOrdinal)),
                CreatedAt = reader.GetDateTime(createdAtOrdinal),
                UpdatedAt = reader.IsDBNull(updatedAtOrdinal) ? null : reader.GetDateTime(updatedAtOrdinal),
                Images = new List<Image>()
            };

            // Populate the post's associated images
            List<Image> postImages = new List<Image>();
            do
            {
                if (!reader.IsDBNull(postImageIdOrdinal))
                {
                    Image postImage = new Image
                    {
                        Id = reader.GetInt32(postImageIdOrdinal),
                        FileName = reader.GetString(postImageFileNameOrdinal),
                        AltText = reader.GetString(postImageAltTextOrdinal),
                        ImageData = (byte[])reader.GetValue(postImageDataOrdinal),
                        Height = reader.GetInt32(postImageHeightOrdinal),
                        Width = reader.GetInt32(postImageWidthOrdinal),
                        FileSize = reader.GetInt32(postImageFileSizeOrdinal),
                        CreatedAt = reader.GetDateTime(postImageCreatedAtOrdinal)
                    };

                    post.Images.Add(postImage);
                }

            } while (await reader.ReadAsync(cancellationToken));

            return post;
        }

        public Task<Post> UpdateAsync(int id, Post entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves a post along with its associated images to the database and returns the saved post with its generated ID and creation timestamp.
        /// </summary>
        /// <param name="post">The post to save.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The saved post with its generated ID and creation timestamp.</returns>
        public async Task<Post> WriteAsync(Post post, CancellationToken cancellationToken)
        {
            const string sql = """
            INSERT INTO posts (author_id, title, content)
            VALUES (@author_id, @title, @content);

            SELECT post_id, created_at
            FROM posts
            WHERE post_id = LAST_INSERT_ID();
            """;

            await using MySqlConnection connection = await _dbConnect.ConnectAsync(cancellationToken);

            await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                // Step 1: Insert the post into the database
                await using MySqlCommand command = new MySqlCommand(sql, connection, transaction);

                command.Parameters.AddWithValue("@author_id", post.Author.Id);
                command.Parameters.AddWithValue("@title", post.Title);
                command.Parameters.AddWithValue("@content", post.Content);

                await using DbDataReader reader1 = await command.ExecuteReaderAsync();
    
                // Check if the reader has rows before reading
                if (!await reader1.ReadAsync())
                {
                    throw new Exception("Dit opslag kunne ikke gemmes, prøv igen!");
                }

                // Populate with values generated by the database (post_id and created_at)
                post.Id = reader1.GetInt32(reader1.GetOrdinal("post_id"));
                post.CreatedAt = reader1.GetDateTime(reader1.GetOrdinal("created_at"));

                await reader1.CloseAsync();

                foreach (Image image in post.Images)
                {
                    // Step 2: Insert each associated image into the database and link them to the post
                    const string sql2 = """
                    INSERT INTO images (file_name, alt_text, image_data, height, width, file_size)
                    VALUES (@file_name, @alt_text, @image_data, @height, @width, @file_size);
                    
                    SELECT image_id, created_at
                    FROM images
                    WHERE image_id = LAST_INSERT_ID();
                    """;

                    await using MySqlCommand command2 = new MySqlCommand(sql2, connection, transaction);

                    command2.Parameters.AddWithValue("@file_name", image.FileName);
                    command2.Parameters.AddWithValue("@alt_text", image.AltText);
                    command2.Parameters.AddWithValue("@image_data", image.ImageData);
                    command2.Parameters.AddWithValue("@height", image.Height);
                    command2.Parameters.AddWithValue("@width", image.Width);
                    command2.Parameters.AddWithValue("@file_size", image.FileSize);

                    await using DbDataReader reader2 = await command2.ExecuteReaderAsync();

                    // Check if the reader has rows before reading
                    if (!await reader2.ReadAsync())
                    {
                        throw new Exception("Dit opslag kunne ikke gemmes, prøv igen!");
                    }

                    // Populate with values generated by the database (image_id and created_at)
                    image.Id = reader2.GetInt32(reader2.GetOrdinal("image_id"));
                    image.CreatedAt = reader2.GetDateTime(reader2.GetOrdinal("created_at"));

                    await reader2.CloseAsync();

                    // Step 3: Associate the saved image with the post in the junction table
                    const string sql3 = """
                    INSERT INTO post_images (post_id, image_id)
                    VALUES (@post_id, @image_id);
                    """;

                    await using MySqlCommand command3 = new MySqlCommand(sql3, connection, transaction);
                    command3.Parameters.AddWithValue("@post_id", post.Id);
                    command3.Parameters.AddWithValue("@image_id", image.Id);
                    int rowsAffected = await command3.ExecuteNonQueryAsync();

                    // Check if the association was successful
                    if (rowsAffected == 0)
                    {
                        throw new Exception("Dit opslag kunne ikke gemmes, prøv igen!");
                    }
                }

                // Commit the transaction if everything is successful
                await transaction.CommitAsync();
                return post;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public Post Delete(int id)
        {
            throw new NotImplementedException();
        }
        public List<Post> ReadAll()
        {
            throw new NotImplementedException();
        }
        public Post? ReadById(int id)
        {
            throw new NotImplementedException();
        }
        public Post Update(int id, Post entity)
        {
            throw new NotImplementedException();
        }

        public Post Write(Post entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves a list of posts for the news feed paginated based on the provided query options.
        /// The method fetches posts along with their associated author and images, ordered by creation date in descending order.
        /// </summary>
        /// <param name="paginationOptions">The pagination options</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A list of posts for the news feed</returns>
        public async Task<List<Post>> ReadNewsFeedAsync(PaginationOptions paginationOptions, CancellationToken cancellationToken)
        {
            const string sql = """
            SELECT
                p.post_id,
                p.author_id,
                p.title,
                p.content,
                p.created_at,
                p.updated_at,

                COALESCE(likes.likes_count, 0) AS likes_count,

                u.user_id,
                CONCAT_WS(' ', s.first_name, s.last_name) AS display_name,

                user_image.image_id AS profile_image_id,
                user_image.file_name AS profile_image_file_name,
                user_image.alt_text AS profile_image_alt_text,
                user_image.image_data AS profile_image_data,
                user_image.height AS profile_image_height,
                user_image.width AS profile_image_width,

                i.image_id AS post_image_id,
                i.file_name AS post_image_file_name,
                i.alt_text AS post_image_alt_text,
                i.image_data AS post_image_data,
                i.height AS post_image_height,
                i.width AS post_image_width,
                i.file_size AS post_image_file_size,
                i.created_at AS post_image_created_at

            FROM (
                SELECT
                    post_id,
                    author_id,
                    title,
                    content,
                    created_at,
                    updated_at
                FROM posts
                WHERE deleted_at IS NULL
                ORDER BY created_at DESC, post_id DESC
                LIMIT @page_size OFFSET @offset
            ) p                                                              -- Subquery to paginate the posts before joining with other 1:M tables, ensuring that the correct number of posts are fetched

            LEFT JOIN (
                SELECT
                    post_id,
                    COUNT(*) AS likes_count
                FROM user_likes_post
                GROUP BY post_id
            ) likes ON p.post_id = likes.post_id                             -- Join the user_likes_post junction table to get the count of likes for each post with a subquery to avoid unnecessary row multiplication

            LEFT JOIN post_images pi ON p.post_id = pi.post_id               -- Join the post_images junction table to get the associated images for the post

            LEFT JOIN images i ON pi.image_id = i.image_id                   -- Join the images table to get the details of each image associated with the post

            LEFT JOIN users u ON p.author_id = u.user_id                     -- Join the users table to get the author details for the post

            LEFT JOIN students AS s ON u.user_id = s.user_id                 -- Join the students table to get the display name of the author

            LEFT JOIN images user_image ON u.image_id = user_image.image_id  -- Join the images table to get the profile image of the author

            ORDER BY p.created_at DESC, p.post_id DESC;
            """;

            await using MySqlConnection connection = await _dbConnect.ConnectAsync(cancellationToken);
            await using MySqlCommand command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@page_size", paginationOptions.PageSize);
            command.Parameters.AddWithValue("@offset", (paginationOptions.PageNumber - 1) * paginationOptions.PageSize);

            await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

            // Cache column ordinals
            int postIdOrdinal = reader.GetOrdinal("post_id");
            int userIdOrdinal = reader.GetOrdinal("user_id");
            int displayNameOrdinal = reader.GetOrdinal("display_name");
            int titleOrdinal = reader.GetOrdinal("title");
            int contentOrdinal = reader.GetOrdinal("content");
            int likesCountOrdinal = reader.GetOrdinal("likes_count");
            int createdAtOrdinal = reader.GetOrdinal("created_at");
            int updatedAtOrdinal = reader.GetOrdinal("updated_at");

            int profileImageIdOrdinal = reader.GetOrdinal("profile_image_id");
            int profileImageFileNameOrdinal = reader.GetOrdinal("profile_image_file_name");
            int profileImageAltTextOrdinal = reader.GetOrdinal("profile_image_alt_text");
            int profileImageDataOrdinal = reader.GetOrdinal("profile_image_data");
            int profileImageHeightOrdinal = reader.GetOrdinal("profile_image_height");
            int profileImageWidthOrdinal = reader.GetOrdinal("profile_image_width");

            int postImageIdOrdinal = reader.GetOrdinal("post_image_id");
            int postImageFileNameOrdinal = reader.GetOrdinal("post_image_file_name");
            int postImageAltTextOrdinal = reader.GetOrdinal("post_image_alt_text");
            int postImageDataOrdinal = reader.GetOrdinal("post_image_data");
            int postImageHeightOrdinal = reader.GetOrdinal("post_image_height");
            int postImageWidthOrdinal = reader.GetOrdinal("post_image_width");
            int postImageFileSizeOrdinal = reader.GetOrdinal("post_image_file_size");
            int postImageCreatedAtOrdinal = reader.GetOrdinal("post_image_created_at");

            List<Post> posts = new List<Post>();

            Post? currentPost = null;

            while (await reader.ReadAsync(cancellationToken))
            {
                int postId = reader.GetInt32(postIdOrdinal);

                // Check whether we've reached a new post
                if (currentPost == null || currentPost.Id != postId)
                {
                    // Populate the author's profile image if one exists
                    Image? profileImage = null;

                    if (!reader.IsDBNull(profileImageIdOrdinal))
                    {
                        profileImage = new Image
                        {
                            Id = reader.GetInt32(profileImageIdOrdinal),
                            FileName = reader.GetString(profileImageFileNameOrdinal),
                            AltText = reader.GetString(profileImageAltTextOrdinal),
                            ImageData = (byte[])reader.GetValue(profileImageDataOrdinal),
                            Height = reader.GetInt32(profileImageHeightOrdinal),
                            Width = reader.GetInt32(profileImageWidthOrdinal)
                        };
                    }

                    // Populate the author's details
                    UserSummary author = new UserSummary
                    {
                        Id = reader.GetInt32(userIdOrdinal),
                        DisplayName = reader.GetString(displayNameOrdinal),
                        ProfileImage = profileImage
                    };

                    // Create the new post
                    currentPost = new Post
                    {
                        Id = postId,
                        Author = author,
                        Title = reader.GetString(titleOrdinal),
                        Content = reader.GetString(contentOrdinal),
                        LikesCount = Convert.ToInt32(reader.GetValue(likesCountOrdinal)),
                        CreatedAt = reader.GetDateTime(createdAtOrdinal),
                        UpdatedAt = reader.IsDBNull(updatedAtOrdinal) ? null : reader.GetDateTime(updatedAtOrdinal),
                        Images = new List<Image>()
                    };

                    posts.Add(currentPost);
                }

                // Populate the current post's associated images
                if (!reader.IsDBNull(postImageIdOrdinal))
                {
                    Image postImage = new Image
                    {
                        Id = reader.GetInt32(postImageIdOrdinal),
                        FileName = reader.GetString(postImageFileNameOrdinal),
                        AltText = reader.GetString(postImageAltTextOrdinal),
                        ImageData = (byte[])reader.GetValue(postImageDataOrdinal),
                        Height = reader.GetInt32(postImageHeightOrdinal),
                        Width = reader.GetInt32(postImageWidthOrdinal),
                        FileSize = reader.GetInt32(postImageFileSizeOrdinal),
                        CreatedAt = reader.GetDateTime(postImageCreatedAtOrdinal)
                    };

                    currentPost.Images.Add(postImage);
                }
            }

            return posts;
        }

        public Task<List<Post>> ReadRecommendedFeedAsync(PaginationOptions paginationOptions, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
