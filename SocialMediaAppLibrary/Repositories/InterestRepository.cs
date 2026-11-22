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
    
    public Interest Write(Interest entity)
    {
        throw new NotImplementedException();
    }

    public List<Interest> ReadAll()
    {
        try
        {
            string sql = """
                         SELECT * FROM interests
                         """;

            using MySqlConnection conn = _db.Connect();

            using MySqlCommand cmd = new MySqlCommand(sql, conn);
            
            using MySqlDataReader reader = cmd.ExecuteReader();

            List<Interest> interestList = new List<Interest>();
            while (reader.Read())
            {
                Interest interest = new Interest();
                interest.Id = reader.GetInt32("interest_id");
                interest.Name = reader.GetString("name");
                
                interestList.Add(interest);
            }

            return interestList;
        }
        catch (Exception ex)
        {
            throw new Exception("Coudn't read all interests (Repo)", ex);
        }
    }

    public Interest Delete(int id)
    {
        throw new NotImplementedException();
    }

    public Interest? ReadById(int id)
    {
        throw new NotImplementedException();
    }

    public Interest Update(int id, Interest entity)
    {
        throw new NotImplementedException();
    }
}