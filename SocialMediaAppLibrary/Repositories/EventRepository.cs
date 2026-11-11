using SocialMediaAppLibrary.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repository
{
    public class EventRepository
    {
        private readonly DbConnect _dbConnect;

        public EventRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

    }
}
