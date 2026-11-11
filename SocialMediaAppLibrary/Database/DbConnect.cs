using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using DotNetEnv;

namespace SocialMediaAppLibrary.Database
{
    public class DbConnect
    {
        private string _connectionString;
        public DbConnect()
        {
            // Walks up from the current directory looking for a .env file
            Env.TraversePath().Load();

            _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                ?? throw new InvalidOperationException("CONNECTION_STRING environment variable is not set.");
        }
        public MySqlConnection Connect()     // brug denne uden properties - den returnerer en MySqlCOnnection automatisk når metoden kaldes, så den gemmes bare i en variabel
        {
            var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

    }
}
