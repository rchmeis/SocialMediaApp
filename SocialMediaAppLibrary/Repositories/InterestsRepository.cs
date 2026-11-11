using SocialMediaAppLibrary.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repository
{
    public class InterestsRepository
    {

        private readonly DbConnect _dbConnect;

        public InterestsRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

    }
}
