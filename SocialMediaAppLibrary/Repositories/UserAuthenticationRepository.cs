using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;
using SocialMediaAppLibrary.Repositories.Interfaces;
using MySql.Data.MySqlClient;
using System.Threading;

namespace SocialMediaAppLibrary.Repositories
{
    /// <summary>
    /// Provides MySQL data access operations for <see cref="UserAuthentication"/> records.
    /// </summary>
    public class UserAuthenticationRepository : IUserAuthenticationRepository
    {

        private readonly IDbConnect _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuthenticationRepository"/> class.
        /// </summary>
        /// <param name="dbConnect">The database connection provider used to open MySQL connections.</param>
        public UserAuthenticationRepository(IDbConnect dbConnect)
        {
            _db = dbConnect;
        }

        public UserAuthentication Delete(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks whether an account already exists with the specified email address or username
        /// in a single database query.
        /// </summary>
        /// <param name="email">The email address candidate to check for duplicates.</param>
        /// <param name="username">The username candidate to check for duplicates.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="DuplicateAccountCheck"/> indicating whether the email and/or username
        /// already exists in the <c>user_authentications</c> table.
        /// </returns>
        public async Task<DuplicateAccountCheck> CheckDuplicateAccountAsync(string email, string username, CancellationToken cancellationToken=default)
        {
            string normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
            string normalizedUsername = (username ?? string.Empty).Trim().ToLowerInvariant();

            await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);

            string query = """
                SELECT
                    EXISTS(
                        SELECT 1
                        FROM user_authentications
                        WHERE LOWER(email) = (@Email)
                        AND deleted_at IS NULL
                    ) AS email_exists,
                    EXISTS(
                        SELECT 1
                        FROM user_authentications
                        WHERE LOWER(username) = (@Username)
                        AND deleted_at IS NULL
                    ) AS username_exists;
                """;

            await using MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Email", normalizedEmail);
            cmd.Parameters.AddWithValue("@Username", normalizedUsername);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return new DuplicateAccountCheck(false, false);
            }

            int emailOrdinal = reader.GetOrdinal("email_exists");
            int usernameOrdinal = reader.GetOrdinal("username_exists");

            bool emailExists = !await reader.IsDBNullAsync(emailOrdinal)
                && reader.GetBoolean(emailOrdinal);

            bool usernameExists = !await reader.IsDBNullAsync(usernameOrdinal)
                && reader.GetBoolean(usernameOrdinal);

            return new DuplicateAccountCheck(emailExists, usernameExists);
        }


        /// <summary>
        /// Retrieves a <see cref="UserAuthentication"/> record matching either the given email address or username.
        /// </summary>
        /// <param name="input">The email address or username to look up.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// The matching <see cref="UserAuthentication"/> instance if found; otherwise, <see langword="null"/>.
        /// </returns>
        public async Task<UserAuthentication?> GetByEmailOrUsernameAsync(string input, CancellationToken cancellationToken=default)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            await using MySqlConnection connection = await _db.ConnectAsync(cancellationToken);
            const string query = """
                SELECT
                        user_authentication_id,
                        username,
                        email,
                        password_hash,
                        is_verified,
                        role,
                        created_at,
                        deleted_at
                FROM user_authentications
                WHERE email = @input OR username = @input AND deleted_at IS NULL
                LIMIT 1;
                """;

            using MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@input", input.Trim());
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            //because we use await cmd.ExecuteReaderAsync, the type of reader changes to DbDataReader which only takes an int column index! thats why we cant use GetString(string) as we do with cmd.ExecuteReader();
            int deletedAtOrdinal = reader.GetOrdinal("deleted_at");
            int idOrdinal = reader.GetOrdinal("user_authentication_id");
            int usernameOrdinal = reader.GetOrdinal("username");
            int emailOrdinal = reader.GetOrdinal("email");
            int passwordHashOrdinal = reader.GetOrdinal("password_hash");
            int isVerifiedOrdinal = reader.GetOrdinal("is_verified");
            int roleOrdinal = reader.GetOrdinal("role");
            int createdAtOrdinal = reader.GetOrdinal("created_at");
           

            string roleValue = reader.GetString(roleOrdinal);

            return new UserAuthentication
            {
                Id = reader.GetInt32(idOrdinal),
                Username = reader.GetString(usernameOrdinal),
                Email = reader.GetString(emailOrdinal),
                PasswordHashed = reader.GetString(passwordHashOrdinal),
                IsVerified = reader.GetBoolean(isVerifiedOrdinal),
                Role = Enum.Parse<Role>(roleValue, ignoreCase: true),
                CreatedAt = reader.GetDateTime(createdAtOrdinal),
                DeletedAt = reader.IsDBNull(deletedAtOrdinal)
                    ? null
                    : reader.GetDateTime(deletedAtOrdinal)
            };

        }


        public List<UserAuthentication> ReadAll()
        {
            throw new NotImplementedException();
        }

        public UserAuthentication? ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public UserAuthentication Update(int id, UserAuthentication entity)
        {
            throw new NotImplementedException();
        }

        public UserAuthentication Write(UserAuthentication entity)
        {
            try
            {
                string sql = """
                             INSERT INTO user_authentications (user_authentication_id, username, email, password_hash, is_verified, role, created_at, deleted_at) 
                             VALUES (@user_authentication_id, @username, @email, @password_hash, @is_verified, @role, @created_at, @deleted_at)
                             """;

                MySqlConnection conn = _db.Connect();

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("user_authentication_id", entity.Id);
                cmd.Parameters.AddWithValue("@username", entity.Username);
                cmd.Parameters.AddWithValue("@email", entity.Email);
                cmd.Parameters.AddWithValue("@password_hash", entity.PasswordHashed);
                cmd.Parameters.AddWithValue("@is_verified", entity.IsVerified);
                cmd.Parameters.AddWithValue("@role", entity.Role.ToString());
                cmd.Parameters.AddWithValue("@created_at", entity.CreatedAt);
                cmd.Parameters.AddWithValue("@deleted_at", entity.DeletedAt);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong writing user authentication into DB:", ex);
            }

            return entity;
        }
    }
}
