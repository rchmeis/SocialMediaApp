using MySql.Data.MySqlClient;
using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    public class InstitutionRepository : IInstitutionRepository
    {
        private readonly IDbConnect _db;
        public InstitutionRepository(IDbConnect db)
        {
            _db = db;
        }
        public Institute Delete(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves all distinct, non-empty institution email domain names from the database.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A list of trimmed institution domain names; returns an empty list if none are configured.
        /// </returns>
        public async Task<List<string>> ReadAllDomainsAsync(CancellationToken cancellationToken = default)
        {
            List<string> domains = new List<string>();

            await using MySqlConnection connection = _db.Connect();           

            const string query = """
                SELECT DISTINCT domain_name
                FROM institutes
                WHERE domain_name IS NOT NULL;
                """;

            await using MySqlCommand cmd = new MySqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            int domainOrdinal = reader.GetOrdinal("domain_name");

            while (await reader.ReadAsync(cancellationToken))
            {
                if (!await reader.IsDBNullAsync(domainOrdinal, cancellationToken))
                {
                    string domain = reader.GetString(domainOrdinal).Trim();
                    if (!string.IsNullOrWhiteSpace(domain))
                    {
                        domains.Add(domain);
                    }
                }
            }

            return domains;
        }

        public List<Institute> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Institute? ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public Institute Update(int id, Institute entity)
        {
            throw new NotImplementedException();
        }

        public Institute Write(Institute entity)
        {
            throw new NotImplementedException();
        }

      
        
    }
}
