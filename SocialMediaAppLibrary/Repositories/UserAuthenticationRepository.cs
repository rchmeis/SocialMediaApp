using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;
using SocialMediaAppLibrary.Repositories.Interfaces;
using MySql.Data.MySqlClient;

namespace SocialMediaAppLibrary.Repositories
{
    public class UserAuthenticationRepository : IUserAuthenticationRepository
    {

        private readonly IDbConnect _db;

        public UserAuthenticationRepository(IDbConnect dbConnect)
        {
            _db = dbConnect;
        }

        public UserAuthentication Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<UserAuthentication> GetByEmailOrUsernameAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            using MySqlConnection connection = _db.Connect();
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
                WHERE email = @input OR username = @input
                LIMIT 1;
                """;

            using MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@input", input);
            using MySqlDataReader reader = cmd.ExecuteReader();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            int deletedAtOrdinal = reader.GetOrdinal("deleted_at");
            string roleValue = reader.GetString("role");

            return new UserAuthentication
            {
                Id = reader.GetInt32("user_authentication_id"),
                Username = reader.GetString("username"),
                Email = reader.GetString("email"),
                PasswordHashed = reader.GetString("password_hash"),
                IsVerified = reader.GetBoolean("is_verified"),
                Role = Enum.Parse<Role>(roleValue, ignoreCase: true),
                CreatedAt = reader.GetDateTime("created_at"),
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
            throw new NotImplementedException();
        }
    }
}
