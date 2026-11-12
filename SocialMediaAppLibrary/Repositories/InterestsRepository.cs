using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repository
{
    public class InterestsRepository: IRead<Interest, int>
    {

        private readonly DbConnect _dbConnect;

        public InterestsRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        public List<Interest> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Interest ReadById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
