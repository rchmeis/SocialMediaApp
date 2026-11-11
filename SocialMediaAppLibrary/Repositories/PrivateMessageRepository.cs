using SocialMediaAppLibrary.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repository
{
    public class PrivateMessageRepository
    {

        private readonly DbConnect _dbConnect;

        public PrivateMessageRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

    }
}
