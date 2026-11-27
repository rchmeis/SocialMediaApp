using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Repositories.Interfaces;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories;

public class InterestRepository : IInterestRepository
{
    private readonly IDbConnect _db;

    public InterestRepository(IDbConnect db)
    {
        _db = db;
    }


    public async Task<List<Interest>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = """
            SELECT interest_id, name
            FROM interests;
            """;

            await using MySqlConnection conn = await _db.ConnectAsync(cancellationToken);
            await using MySqlCommand cmd = new MySqlCommand(sql, conn);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            int idOrdinal = reader.GetOrdinal("interest_id");
            int nameOrdinal = reader.GetOrdinal("name");

            List<Interest> interestList = new List<Interest>();
            while (await reader.ReadAsync(cancellationToken))
            {
                Interest interest = new Interest
                {
                    Id = reader.GetInt32(idOrdinal),
                    Name = reader.GetString(nameOrdinal)
                };

                interestList.Add(interest);
            }

            return interestList;
        }
        catch (Exception ex)
        {
            throw new Exception("Couldn't read all interests (Repo)", ex);
        }
    }

    public Task<Interest> WriteAsync(Interest entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Interest> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }


    public Task<Interest?> ReadByIdAsync(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Interest> UpdateAsync(int id, Interest entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}