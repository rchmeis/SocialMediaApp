using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Database
{
    public interface IDbConnect
    {
        public MySqlConnection Connect();
        Task<MySqlConnection> ConnectAsync(CancellationToken cancellationToken = default);
    }
}
