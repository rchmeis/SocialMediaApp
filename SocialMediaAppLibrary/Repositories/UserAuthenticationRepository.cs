using SocialMediaAppLibrary.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repository
{
    public class UserAuthenticationRepository
    {

        private readonly DbConnect _dbConnect;

        public UserAuthenticationRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

    }
}
