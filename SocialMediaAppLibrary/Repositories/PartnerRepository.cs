using SocialMediaAppLibrary.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repository
{
    public class PartnerRepository
    {

        private readonly DbConnect _dbConnect;

        public PartnerRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

    }
}
