using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    public class PartnerRepository: IPartnerRepository
    {

        private readonly DbConnect _dbConnect;

        public PartnerRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        public Partner Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Partner> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Partner? ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public Partner Update(int id, Partner entity)
        {
            throw new NotImplementedException();
        }

        public Partner Write(Partner entity)
        {
            throw new NotImplementedException();
        }
    }
}
