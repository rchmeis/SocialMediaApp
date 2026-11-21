using DotNetEnv;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Database
{
    public class DbConnectTest : IDbConnect
    {
        private string _connectionString;
        public DbConnectTest()
        {
            // Walks up from the current directory looking for a .env file
            Env.TraversePath().Load();

            _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING_TEST")
                ?? throw new InvalidOperationException("CONNECTION_STRING_TEST environment variable is not set.");
        }
        public MySqlConnection Connect()
        {
            var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
        public async Task<MySqlConnection> ConnectAsync(CancellationToken cancellationstoken = default)     // brug denne uden properties - den returnerer en MySqlCOnnection automatisk når metoden kaldes, så den gemmes bare i en variabel
        {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationstoken);
            return connection;
        }
    }
}
