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

        /// <summary>
        /// Soft-deletes a post by setting its <c>deleted_at</c> timestamp and returns the updated post with author details.
        /// </summary>
        /// <param name="id">The identifier of the post to soft-delete.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation if the request aborts.</param>
        /// <returns>The updated <see cref="Post"/> record with a populated <see cref="UserSummary"/> author, or <c>null</c> if no active post with the specified ID exists.</returns>
        public async Task<Post?> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            const string updateSql = """
                    UPDATE posts
                    SET deleted_at = CURRENT_TIMESTAMP(6),
                        updated_at = CURRENT_TIMESTAMP(6)
                    WHERE post_id = @postId
                      AND deleted_at IS NULL;
                    """;

            const string selectSql = """
                    SELECT
                        p.post_id,
                        p.author_id,
                        p.title,
                        p.content,
                        p.created_at,
                        p.updated_at,
                        p.deleted_at,

                        u.user_id,
                        CONCAT_WS(' ', s.first_name, s.last_name) AS display_name,

                        user_image.image_id AS profile_image_id,
                        user_image.file_name AS profile_image_file_name,
                        user_image.alt_text AS profile_image_alt_text,
                        user_image.height AS profile_image_height,
                        user_image.width AS profile_image_width
                    FROM posts p
                    LEFT JOIN users u ON p.author_id = u.user_id
                    LEFT JOIN students s ON u.user_id = s.user_id
                    LEFT JOIN images user_image ON u.image_id = user_image.image_id
                    WHERE p.post_id = @postId;
                    """;

            await using MySqlConnection connection = await _dbConnect.ConnectAsync(cancellationToken);

            await using MySqlCommand updateCommand = new MySqlCommand(updateSql, connection);
            updateCommand.Parameters.AddWithValue("@postId", id);
            int rowsAffected = await updateCommand.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                return null;
            }

            await using MySqlCommand selectCommand = new MySqlCommand(selectSql, connection);
            selectCommand.Parameters.AddWithValue("@postId", id);

            await using DbDataReader reader = await selectCommand.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            int postIdOrdinal = reader.GetOrdinal("post_id");
            int authorIdOrdinal = reader.GetOrdinal("author_id");
            int titleOrdinal = reader.GetOrdinal("title");
            int contentOrdinal = reader.GetOrdinal("content");
            int createdAtOrdinal = reader.GetOrdinal("created_at");
            int updatedAtOrdinal = reader.GetOrdinal("updated_at");
            int deletedAtOrdinal = reader.GetOrdinal("deleted_at");

            int userIdOrdinal = reader.GetOrdinal("user_id");
            int displayNameOrdinal = reader.GetOrdinal("display_name");

            int profileImageIdOrdinal = reader.GetOrdinal("profile_image_id");
            int profileImageFileNameOrdinal = reader.GetOrdinal("profile_image_file_name");
            int profileImageAltTextOrdinal = reader.GetOrdinal("profile_image_alt_text");
            int profileImageHeightOrdinal = reader.GetOrdinal("profile_image_height");
            int profileImageWidthOrdinal = reader.GetOrdinal("profile_image_width");

            Image? profileImage = null;
            if (!reader.IsDBNull(profileImageIdOrdinal))
            {
                profileImage = new Image
                {
                    Id = reader.GetInt32(profileImageIdOrdinal),
                    FileName = reader.GetString(profileImageFileNameOrdinal),
                    AltText = reader.IsDBNull(profileImageAltTextOrdinal) ? string.Empty : reader.GetString(profileImageAltTextOrdinal),
                    Height = reader.GetInt32(profileImageHeightOrdinal),
                    Width = reader.GetInt32(profileImageWidthOrdinal)
                };
            }

            int authorId = reader.IsDBNull(userIdOrdinal) ? reader.GetInt32(authorIdOrdinal) : reader.GetInt32(userIdOrdinal);
            string displayName = reader.IsDBNull(displayNameOrdinal) ? string.Empty : reader.GetString(displayNameOrdinal);

            UserSummary author = new UserSummary
            {
                Id = authorId,
                DisplayName = displayName,
                ProfileImage = profileImage
            };

            return new Post
            {
                Id = reader.GetInt32(postIdOrdinal),
                Author = author,
                Title = reader.IsDBNull(titleOrdinal) ? null : reader.GetString(titleOrdinal),
                Content = reader.GetString(contentOrdinal),
                CreatedAt = reader.GetDateTime(createdAtOrdinal),
                UpdatedAt = reader.IsDBNull(updatedAtOrdinal) ? null : reader.GetDateTime(updatedAtOrdinal),
                DeletedAt = reader.IsDBNull(deletedAtOrdinal) ? null : reader.GetDateTime(deletedAtOrdinal)
            };
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
                user_image.height     AS profile_image_height,
                user_image.width      AS profile_image_width,

                i.image_id   AS post_image_id,
                i.file_name  AS post_image_file_name,
                i.alt_text   AS post_image_alt_text,
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
            int profileImageHeightOrdinal = reader.GetOrdinal("profile_image_height");
            int profileImageWidthOrdinal = reader.GetOrdinal("profile_image_width");

            int postImageIdOrdinal = reader.GetOrdinal("post_image_id");
            int postImageFileNameOrdinal = reader.GetOrdinal("post_image_file_name");
            int postImageAltTextOrdinal = reader.GetOrdinal("post_image_alt_text");
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
                Title = reader.IsDBNull(titleOrdinal) ? null : reader.GetString(titleOrdinal),
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

        /// <summary>
        /// Updates a post's title, content and <c>updated_at</c> timestamp, and inserts any new images
        /// linked to the post. All changes run in a single transaction, so either everything is saved or nothing is.
        /// </summary>
        /// <remarks>
        /// Only images with an <see cref="Image.Id"/> of 0 are inserted; existing images are left unchanged.
        /// Each new image is saved to the <c>images</c> table and linked to the post through <c>post_images</c>.
        /// Removing images is handled separately by <c>ImageRepository.DeleteAsync</c>.
        /// </remarks>
        /// <param name="id">The unique identifier of the post to update.</param>
        /// <param name="entity">The post containing the updated title, content and images.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The updated post, with ids assigned to the newly inserted images.</returns>
        /// <exception cref="Exception">Thrown when the post could not be updated. No changes are saved.</exception>
        public async Task<Post> UpdateAsync(int id, Post entity, CancellationToken cancellationToken)
        {
            string sqlPosts = """
                              UPDATE posts
                              SET 
                              title = @title,
                              content = @content,
                              updated_at = @updated_at
                              WHERE post_id = @post_id
                              """;
            
            await using MySqlConnection connection = await _dbConnect.ConnectAsync();
            
            await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                // Step 1: update post table
                await using MySqlCommand cmdPosts = new MySqlCommand(sqlPosts, connection, transaction);

                cmdPosts.Parameters.AddWithValue("title", entity.Title);
                cmdPosts.Parameters.AddWithValue("content", entity.Content);
                cmdPosts.Parameters.AddWithValue("updated_at", DateTime.Now);
                cmdPosts.Parameters.AddWithValue("post_id", entity.Id);

                await cmdPosts.ExecuteNonQueryAsync(cancellationToken);

                // Step 2: Udate images 
                foreach (Image image in entity.Images.Where(i => i.Id == 0))
                {
                    // Insert the image itself
                    const string sqlImages = """
                                             INSERT INTO images (file_name, alt_text, image_data, height, width, file_size)
                                             VALUES (@file_name, @alt_text, @image_data, @height, @width, @file_size);
                                             """;

                    await using MySqlCommand cmdImages = new MySqlCommand(sqlImages, connection, transaction);
                    cmdImages.Parameters.AddWithValue("@file_name", image.FileName);
                    cmdImages.Parameters.AddWithValue("@alt_text", image.AltText);
                    cmdImages.Parameters.AddWithValue("@image_data", image.ImageData);
                    cmdImages.Parameters.AddWithValue("@height", image.Height);
                    cmdImages.Parameters.AddWithValue("@width", image.Width);
                    cmdImages.Parameters.AddWithValue("@file_size", image.FileSize);

                    await cmdImages.ExecuteNonQueryAsync(cancellationToken);
                    image.Id = (int)cmdImages.LastInsertedId;

                    // Link it to the post
                    const string sqlPostImages = """
                                                 INSERT INTO post_images (post_id, image_id)
                                                 VALUES (@post_id, @image_id);
                                                 """;

                    await using MySqlCommand cmdPostImages = new MySqlCommand(sqlPostImages, connection, transaction);
                    cmdPostImages.Parameters.AddWithValue("@post_id", entity.Id);
                    cmdPostImages.Parameters.AddWithValue("@image_id", image.Id);
                    await cmdPostImages.ExecuteNonQueryAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't update Post", ex);
            }

            return entity;
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
        public async Task<GenericPaginatedResult<Post>> ReadNewestFeedAsync(PaginationOptions paginationOptions, CancellationToken cancellationToken)
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
                user_image.height AS profile_image_height,
                user_image.width AS profile_image_width,

                i.image_id AS post_image_id,
                i.file_name AS post_image_file_name,
                i.alt_text AS post_image_alt_text,
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
                LIMIT @page_size_plus_one OFFSET @offset
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

            const string countSql = """
            SELECT COUNT(*)
            FROM posts
            WHERE deleted_at IS NULL;
            """;

            await using MySqlConnection connection = await _dbConnect.ConnectAsync(cancellationToken);

            // Get the total count of posts for pagination purposes
            await using MySqlCommand countCommand = new MySqlCommand(countSql, connection);
            int totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));

            await using MySqlCommand command = new MySqlCommand(sql, connection);

            // One extra post is fetched to determine if there are more posts available for the next page
            command.Parameters.AddWithValue("@page_size_plus_one", paginationOptions.PageSize + 1);
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
            int profileImageHeightOrdinal = reader.GetOrdinal("profile_image_height");
            int profileImageWidthOrdinal = reader.GetOrdinal("profile_image_width");

            int postImageIdOrdinal = reader.GetOrdinal("post_image_id");
            int postImageFileNameOrdinal = reader.GetOrdinal("post_image_file_name");
            int postImageAltTextOrdinal = reader.GetOrdinal("post_image_alt_text");
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
                            AltText = reader.IsDBNull(profileImageAltTextOrdinal) ? string.Empty : reader.GetString(profileImageAltTextOrdinal),
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
                        Title = reader.IsDBNull(titleOrdinal) ? null : reader.GetString(titleOrdinal),
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
                        AltText = reader.IsDBNull(profileImageAltTextOrdinal) ? string.Empty : reader.GetString(profileImageAltTextOrdinal),
                        Height = reader.GetInt32(postImageHeightOrdinal),
                        Width = reader.GetInt32(postImageWidthOrdinal),
                        FileSize = reader.GetInt32(postImageFileSizeOrdinal),
                        CreatedAt = reader.GetDateTime(postImageCreatedAtOrdinal)
                    };

                    currentPost.Images.Add(postImage);
                }
            }

            // Determine whether another page exists
            bool hasMore = posts.Count > paginationOptions.PageSize;

            // Remove the extra post if one was fetched
            if (hasMore)
            {
                posts.RemoveAt(posts.Count - 1);
            }

            return new GenericPaginatedResult<Post>
            {
                Items = posts,
                HasMore = hasMore,
                TotalCount = totalCount,
                PageNumber = paginationOptions.PageNumber,
                PageSize = paginationOptions.PageSize
            };
        }

        public Task<GenericPaginatedResult<Post>> ReadRecommendedFeedAsync(PaginationOptions paginationOptions, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves a paginated list of posts created by a specific user, ordered by creation date in descending order.
        /// </summary>
        /// <param name="userId">The unique identifier of the post author</param>
        /// <param name="paginationOptions">The pagination options</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A paginated list of posts for the specified user</returns>
        public async Task<GenericPaginatedResult<Post>> ReadFeedByIdAsync(int userId, PaginationOptions paginationOptions, CancellationToken cancellationToken)
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
                user_image.height AS profile_image_height,
                user_image.width AS profile_image_width,

                i.image_id AS post_image_id,
                i.file_name AS post_image_file_name,
                i.alt_text AS post_image_alt_text,
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
                  AND author_id = @userId
                ORDER BY created_at DESC, post_id DESC
                LIMIT @page_size_plus_one OFFSET @offset
            ) p                                                              -- Subquery paginates only this user's posts before joining 1:M tables

            LEFT JOIN (
                SELECT
                    post_id,
                    COUNT(*) AS likes_count
                FROM user_likes_post
                GROUP BY post_id
            ) likes ON p.post_id = likes.post_id

            LEFT JOIN post_images pi ON p.post_id = pi.post_id

            LEFT JOIN images i ON pi.image_id = i.image_id

            LEFT JOIN users u ON p.author_id = u.user_id

            LEFT JOIN students AS s ON u.user_id = s.user_id

            LEFT JOIN images user_image ON u.image_id = user_image.image_id

            ORDER BY p.created_at DESC, p.post_id DESC;
            """;

            // Filter countSql by author_id as well so TotalCount matches the user's posts
            const string countSql = """
            SELECT COUNT(*)
            FROM posts
            WHERE deleted_at IS NULL
              AND author_id = @userId;
            """;

            await using MySqlConnection connection = await _dbConnect.ConnectAsync(cancellationToken);

            await using MySqlCommand countCommand = new MySqlCommand(countSql, connection);
            countCommand.Parameters.AddWithValue("@userId", userId);
            //countSql only returns 1 row and 1 column (the count). ExecuteScalarAsync avoids the overhead of creating a full data reader just to grab one scalar value.
            int totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));

            await using MySqlCommand command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@page_size_plus_one", paginationOptions.PageSize + 1);
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
            int profileImageHeightOrdinal = reader.GetOrdinal("profile_image_height");
            int profileImageWidthOrdinal = reader.GetOrdinal("profile_image_width");

            int postImageIdOrdinal = reader.GetOrdinal("post_image_id");
            int postImageFileNameOrdinal = reader.GetOrdinal("post_image_file_name");
            int postImageAltTextOrdinal = reader.GetOrdinal("post_image_alt_text");
            int postImageHeightOrdinal = reader.GetOrdinal("post_image_height");
            int postImageWidthOrdinal = reader.GetOrdinal("post_image_width");
            int postImageFileSizeOrdinal = reader.GetOrdinal("post_image_file_size");
            int postImageCreatedAtOrdinal = reader.GetOrdinal("post_image_created_at");

            List<Post> posts = new List<Post>();
            Post? currentPost = null;

            while (await reader.ReadAsync(cancellationToken))
            {
                int postId = reader.GetInt32(postIdOrdinal);

                if (currentPost == null || currentPost.Id != postId)
                {
                    Image? profileImage = null;

                    if (!reader.IsDBNull(profileImageIdOrdinal))
                    {
                        profileImage = new Image
                        {
                            Id = reader.GetInt32(profileImageIdOrdinal),
                            FileName = reader.GetString(profileImageFileNameOrdinal),
                            AltText = reader.IsDBNull(profileImageAltTextOrdinal) ? string.Empty : reader.GetString(profileImageAltTextOrdinal),
                            Height = reader.GetInt32(profileImageHeightOrdinal),
                            Width = reader.GetInt32(profileImageWidthOrdinal)
                        };
                    }

                    UserSummary author = new UserSummary
                    {
                        Id = reader.GetInt32(userIdOrdinal),
                        DisplayName = reader.GetString(displayNameOrdinal),
                        ProfileImage = profileImage
                    };

                    currentPost = new Post
                    {
                        Id = postId,
                        Author = author,
                        Title = reader.IsDBNull(titleOrdinal) ? null : reader.GetString(titleOrdinal),
                        Content = reader.GetString(contentOrdinal),
                        LikesCount = Convert.ToInt32(reader.GetValue(likesCountOrdinal)),
                        CreatedAt = reader.GetDateTime(createdAtOrdinal),
                        UpdatedAt = reader.IsDBNull(updatedAtOrdinal) ? null : reader.GetDateTime(updatedAtOrdinal),
                        Images = new List<Image>()
                    };

                    posts.Add(currentPost);
                }

                if (!reader.IsDBNull(postImageIdOrdinal))
                {
                    Image postImage = new Image
                    {
                        Id = reader.GetInt32(postImageIdOrdinal),
                        FileName = reader.GetString(postImageFileNameOrdinal),
                        AltText = reader.IsDBNull(postImageAltTextOrdinal) ? null:reader.GetString(postImageAltTextOrdinal),
                        Height = reader.GetInt32(postImageHeightOrdinal),
                        Width = reader.GetInt32(postImageWidthOrdinal),
                        FileSize = reader.GetInt32(postImageFileSizeOrdinal),
                        CreatedAt = reader.GetDateTime(postImageCreatedAtOrdinal)
                    };

                    currentPost.Images.Add(postImage);
                }
            }

            bool hasMore = posts.Count > paginationOptions.PageSize;

            if (hasMore)
            {
                posts.RemoveAt(posts.Count - 1);
            }

            return new GenericPaginatedResult<Post>
            {
                Items = posts,
                HasMore = hasMore,
                TotalCount = totalCount,
                PageNumber = paginationOptions.PageNumber,
                PageSize = paginationOptions.PageSize
            };
        }
    }
}
