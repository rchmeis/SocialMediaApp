using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repository
{
    public class PartnerRepository: ICrud<Partner, int>
    {

        private readonly DbConnect _dbConnect;

        public PartnerRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        Partner IWrite<Partner, int>.Delete(int id)
        {
            throw new NotImplementedException();
        }

        List<Partner> IRead<Partner, int>.ReadAll()
        {
            throw new NotImplementedException();
        }

        Partner? IRead<Partner, int>.ReadById(int id)
        {
            throw new NotImplementedException();
        }

        Partner IWrite<Partner, int>.Update(int id, Partner entity)
        {
            throw new NotImplementedException();
        }

        Partner IWrite<Partner, int>.Write(Partner entity)
        {
            throw new NotImplementedException();
        }
    }
}
